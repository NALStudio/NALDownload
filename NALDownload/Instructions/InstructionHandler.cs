using NALDownload.Enums;
using NALDownload.Exceptions;
using NALDownload.Helpers;
using NALDownload.Instructions.Directories;
using NALDownload.Instructions.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VCDiff.Decoders;
using VCDiff.Includes;

namespace NALDownload.Instructions;
internal static class InstructionHandler
{
    private class HandlerContext : IAsyncDisposable
    {
        public HandlerContext(string directory)
        {
            CurrentPath = new();
            OpenedStream = null;

            _directory = directory;
        }

        private readonly string _directory;

        public Stack<string> CurrentPath { get; }
        public string WholePath => Path.Combine(_directory, Path.Combine(CurrentPath.Reverse().ToArray()));

        private FileStream? _fileStream = null;
        public FileStream? OpenedStream
        {
            get => _fileStream;
            set
            {
                FileStream? newValue = value;
                if (_fileStream != newValue)
                    _fileStream?.Dispose();
                _fileStream = newValue;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_fileStream is not null)
                await _fileStream.DisposeAsync();
        }
    }

    public static async Task ApplyInstructionsOnDirectory(string directory, IEnumerable<Instruction> instructions, Action<string>? onProgressUpdate = null)
    {
        await using HandlerContext context = new(directory);

        foreach (Instruction i in instructions)
        {
            onProgressUpdate?.Invoke($"Executing {i.Type} to path '{context.WholePath}'");

            switch (i.Type)
            {
                case InstructionType.WriteGzip:
                    await WriteGZip(i, context);
                    break;
                case InstructionType.WriteVcdiff:
                    await WriteVCDIFF(i, context);
                    break;
                case InstructionType.VerifyFileHash:
                    bool hashesEqual = await CompareHash(i.Body, SanitizeStream(context.OpenedStream));
                    if (!hashesEqual)
                        throw new VerificationException("File hashes do not match.");
                    break;
                case InstructionType.VerifyDirectoryExists:
                    VerifyDirectoryInstruction verifyDirectory = (VerifyDirectoryInstruction)i;
                    string verifyPath = Path.Combine(context.WholePath, verifyDirectory.Path);
                    if (!Directory.Exists(verifyPath))
                        throw new VerificationException("Directory does not exist.");
                    break;
                case InstructionType.Open:
                    OpenInstruction open = (OpenInstruction)i;
                    context.CurrentPath.Push(open.Path);
                    context.OpenedStream = new FileStream(context.WholePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                    break;
                case InstructionType.CreateFile:
                    CreateFileInstruction createFile = (CreateFileInstruction)i;
                    context.CurrentPath.Push(createFile.Path);
                    context.OpenedStream = new FileStream(context.WholePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);
                    break;
                case InstructionType.Close:
                    context.OpenedStream = null;
                    context.CurrentPath.Pop();
                    break;
                case InstructionType.StepIn:
                    StepInInstruction stepIn = (StepInInstruction)i;
                    context.CurrentPath.Push(stepIn.Path);
                    if (!Directory.Exists(context.WholePath))
                        throw new ArgumentException($"Failed stepping into directory '{context.WholePath}'. No directory found.");
                    break;
                case InstructionType.StepOut:
                    if (!Directory.Exists(context.WholePath))
                        throw new ArgumentException($"Failed stepping out of directory '{context.WholePath}'. Directory does not exist.");
                    context.CurrentPath.Pop();
                    break;
                case InstructionType.CreateDirectory:
                    CreateDirectoryInstruction createDir = (CreateDirectoryInstruction)i;
                    context.CurrentPath.Push(createDir.Path);
                    string createDirWhole = context.WholePath;
                    if (Directory.Exists(createDirWhole))
                        throw new ArgumentException($"Failed creating directory. Directory '{context.WholePath}' exists already.");
                    if (Directory.GetParent(createDirWhole) is null)
                        throw new ArgumentException($"Failed creating directory '{context.WholePath}'. No parent directory found.");
                    Directory.CreateDirectory(createDirWhole);
                    break;
                case InstructionType.DeleteFile:
                    DeleteFileInstruction deleteInstruction = (DeleteFileInstruction)i;
                    string delFilePath = Path.Combine(context.WholePath, deleteInstruction.Path);
                    if (!File.Exists(delFilePath))
                        throw new ArgumentException($"Failed deleting file '{context.WholePath}'. File not found.");
                    File.Delete(delFilePath);
                    break;
                case InstructionType.DeleteDirectory:
                case InstructionType.PurgeDirectory:
                    PathInstruction pathInstruction = (PathInstruction)i;
                    string delDirPath = Path.Combine(context.WholePath, pathInstruction.Path);
                    if (!Directory.Exists(delDirPath))
                        throw new ArgumentException($"Failed deleting directory '{context.WholePath}'. Directory not found.");
                    Directory.Delete(delDirPath, pathInstruction.Type == InstructionType.PurgeDirectory);
                    break;
                default:
                    throw new ArgumentException($"Cannot handle instruction of type: '{i.Type}'.");
            }
        }
    }

    private static FileStream SanitizeStream(FileStream? stream)
    {
        if (stream is null)
            throw new ArgumentException("Failed writing GZip. File not opened.");

        if (stream.Position != 0L)
            throw new Exception("Opened stream position is not reset.");

        if (!stream.CanWrite)
            throw new Exception("Opened stream is not writeable.");

        return stream;
    }

    private static async Task WriteGZip(Instruction instruction, HandlerContext context)
    {
        FileStream stream = SanitizeStream(context.OpenedStream);

        using MemoryStream compressed = new(instruction.Body);
        compressed.Position = 0L;
        await GZipHelper.DecompressFromGzipAsync(compressed, stream);
        await stream.FlushAsync();
    }

    private static async Task WriteVCDIFF(Instruction instruction, HandlerContext context)
    {
        FileStream stream = SanitizeStream(context.OpenedStream);

        using MemoryStream sourceData = new();
        await stream.CopyToAsync(sourceData);
        stream.Position = 0L;

        using MemoryStream diff = new(instruction.Body);
        using MemoryStream newFileData = new();

        VCDiffResult result;
        using (VcDecoder encoder = new(sourceData, diff, newFileData))
            (result, _) = await encoder.DecodeAsync();
        Trace.Assert(result == VCDiffResult.SUCCESS);

        newFileData.Position = 0L;
        await newFileData.CopyToAsync(stream);
        await stream.FlushAsync();
    }

    private static async Task<bool> CompareHash(byte[] hash, Stream stream)
    {
        byte[] fileHash;
        using (MD5 md5 = MD5.Create())
            fileHash = await md5.ComputeHashAsync(stream);
        return hash.SequenceEqual(fileHash);
    }
}
