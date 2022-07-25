using NALDownload.Enums;
using NALDownload.Helpers;
using NALDownload.Instructions.Directories;
using NALDownload.Instructions.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCDiff.Decoders;

namespace NALDownload.Instructions;
internal static class InstructionHandler
{
    private class HandlerContext
    {
        public HandlerContext(string directory)
        {
            CurrentPath = new();
            OpenedStream = null;

            _directory = directory;
        }

        private string _directory;

        public Stack<string> CurrentPath { get; }
        public string WholePath => Path.Combine(_directory, Path.Combine(CurrentPath.ToArray()));
        public FileStream? OpenedStream { get; set; }
    }

    public static async Task ApplyInstructionsOnDirectory(string directory, IEnumerable<Instruction> instructions, Action<string>? onProgressUpdate = null)
    {
        HandlerContext context = new(directory);

        foreach (Instruction i in instructions)
        {
            onProgressUpdate?.Invoke($"Executing {i.Type} to path '{context.WholePath}'");

            switch (i.Type)
            {
                case InstructionType.WriteGzip:
                    await WriteGZip(i, context);
                    context.OpenedStream = null;
                    break;
                case InstructionType.WriteVcdiff:
                    await WriteVCDIFF(i, context);
                    context.OpenedStream = null;
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
                case InstructionType.StepIn:
                    StepInInstruction stepIn = (StepInInstruction)i;
                    context.CurrentPath.Push(stepIn.Path);
                    if (!Directory.Exists(context.WholePath))
                        throw new ArgumentException("Failed stepping into directory. No directory found.");
                    break;
                case InstructionType.StepOut:
                    if (!Directory.Exists(context.WholePath))
                        throw new ArgumentException("Failed stepping out of directory. Directory does not exist.");
                    context.CurrentPath.Pop();
                    break;
                case InstructionType.CreateDirectory:
                    CreateDirectoryInstruction createDir = (CreateDirectoryInstruction)i;
                    context.CurrentPath.Push(createDir.Path);
                    string createDirWhole = context.WholePath;
                    if (Directory.Exists(createDirWhole))
                        throw new ArgumentException("Failed creating directory. Directory exists already.");
                    if (Directory.GetParent(createDirWhole) is not null)
                        throw new ArgumentException("Failed creating directory. No parent directory found.");
                    Directory.CreateDirectory(createDirWhole);
                    break;
                case InstructionType.DeleteFile:
                    string delFilePath = context.WholePath;
                    if (!File.Exists(delFilePath))
                        throw new ArgumentException("Failed deleting file. File not found.");
                    File.Delete(delFilePath);
                    context.CurrentPath.Pop();
                    break;
                case InstructionType.DeleteDirectory:
                    string delDirPath = context.WholePath;
                    if (!Directory.Exists(delDirPath))
                        throw new ArgumentException("Failed deleting directory. Directory not found.");
                    Directory.Delete(delDirPath);
                    context.CurrentPath.Pop();
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
    }

    private static async Task WriteVCDIFF(Instruction instruction, HandlerContext context)
    {
        FileStream stream = SanitizeStream(context.OpenedStream);

        using MemoryStream diff = new(instruction.Body);
        diff.Position = 0L;
        using MemoryStream newFileData = new();
        using (VcDecoder encoder = new(stream, diff, newFileData))
            await encoder.DecodeAsync();

        await newFileData.CopyToAsync(stream);
    }
}
