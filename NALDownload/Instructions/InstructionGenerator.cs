using NALDownload.Helpers;
using NALDownload.Instructions.Directories;
using NALDownload.Instructions.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCDiff.Encoders;
using VCDiff.Includes;

namespace NALDownload.Instructions;
internal static class InstructionGenerator
{
    public static async IAsyncEnumerable<Instruction> EnumerateGeneratedDirectoryInstructionsAsync(string oldVersionDirectoryPath, string newVersionDirectoryPath, string currentDirectoryRelativePath, Action<string>? onProgressUpdate)
    {
        string NewFullPath(string relPath) => Path.Combine(newVersionDirectoryPath, relPath);
        string OldFullPath(string relPath) => Path.Combine(oldVersionDirectoryPath, relPath);

        DirectoryInfo? oldDir = new(OldFullPath(currentDirectoryRelativePath));
        if (!oldDir.Exists)
            oldDir = null;
        DirectoryInfo newDir = new(NewFullPath(currentDirectoryRelativePath));

        #region Subdirectories
        onProgressUpdate?.Invoke($"Scanning subdirectories of directory '{currentDirectoryRelativePath}'...");
        IEnumerable<string>? oldPathSubdirs = (oldDir?.EnumerateDirectories().Select(di => di.Name)) ?? Array.Empty<string>();

        IEnumerable<string> newPathSubdirs = newDir.EnumerateDirectories().Select(di => di.Name);

        string[] subDirs = oldPathSubdirs.Concat(newPathSubdirs).Distinct().ToArray();
        // Force case insensitive paths. Path casing is still taken into account in the current spec.
        Trace.Assert(subDirs.DistinctBy(p => p.ToLowerInvariant()).Count() == subDirs.Length);

        foreach (string dirname in subDirs)
        {
            string dirpath = Path.Combine(currentDirectoryRelativePath, dirname);
            DirectoryInfo newSubdir = new(NewFullPath(dirpath));
            DirectoryInfo oldSubdir = new(OldFullPath(dirpath));

            if (!newSubdir.Exists)
            {
                Trace.Assert(oldSubdir.Exists);
                
                bool isEmpty = !oldSubdir.EnumerateFileSystemInfos().Any();
                if (isEmpty)
                    yield return new DeleteDirectoryInstruction(dirname);
                else
                    yield return new PurgeDirectoryInstruction(dirname);
                continue;
            }

            List<Instruction> childDirInstructions = new();
            await foreach (Instruction i in EnumerateGeneratedDirectoryInstructionsAsync(oldVersionDirectoryPath, newVersionDirectoryPath, dirpath, onProgressUpdate))
                childDirInstructions.Add(i);

            bool steppedIn = false;
            if (!oldSubdir.Exists)
            {
                Trace.Assert(newSubdir.Exists);
                yield return new CreateDirectoryInstruction(dirname);
                steppedIn = true;
            }
            else if (childDirInstructions.Count > 0)
            {
                yield return new StepInInstruction(dirname);
                steppedIn = true;
            }

            foreach (Instruction i in childDirInstructions)
                yield return i;

            if (steppedIn)
                yield return new StepOutInstruction();
        }
        #endregion
        #region Files
        onProgressUpdate?.Invoke($"Scanning files of directory '{currentDirectoryRelativePath}'...");
        IEnumerable<string> oldPathFiles = (oldDir?.EnumerateFiles().Select(fi => fi.Name)) ?? Array.Empty<string>();
        IEnumerable<string> newPathFiles = newDir.EnumerateFiles().Select(fi => fi.Name);

        string[] files = oldPathFiles.Concat(newPathFiles).Distinct().ToArray();
        Trace.Assert(files.DistinctBy(p => p.ToLowerInvariant()).Count() == files.Length);

        foreach (string filename in files)
        {
            string filepath = Path.Combine(currentDirectoryRelativePath, filename);
            FileInfo oldFile = new(OldFullPath(filepath));
            FileInfo newFile = new(NewFullPath(filepath));

            if (!newFile.Exists)
            {
                Trace.Assert(oldFile.Exists);
                yield return new DeleteFileInstruction(filename);
                continue;
            }

            if (oldFile.Exists)
            {
                byte[]? writeBytes = null;
                using (FileStream newData = new(newFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using FileStream oldData = new(oldFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (!StreamContentsEqual(oldData, newData))
                        writeBytes = await CreateVCDiffAsync(oldData, newData);
                }

                if (writeBytes is not null)
                {
                    yield return new OpenInstruction(filename);
                    yield return new WriteVcdiff(writeBytes);
                    yield return new CloseInstruction();
                }
            }
            else
            {
                Trace.Assert(newFile.Exists);
                yield return new CreateFileInstruction(filename);
                byte[] writeBytes;
                using (FileStream newData = new(newFile.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                    writeBytes = await CompressToGzipAsync(newData);
                yield return new WriteGzipInstruction(writeBytes);
                yield return new CloseInstruction();
            }
        }
        #endregion
    }

    private static async Task<byte[]> CompressToGzipAsync(Stream newDataStream)
    {
        byte[] output;

        using (MemoryStream compressionOutput = new())
        {
            await GZipHelper.CompressToGzipAsync(newDataStream, compressionOutput);

            compressionOutput.Position = 0L;
            output = compressionOutput.ToArray();
        }

        return output;
    }

    private static async Task<byte[]> CreateVCDiffAsync(Stream oldDataStream, Stream newDataStream)
    {
        byte[] output;

        using (MemoryStream diffOutput = new())
        {
            VCDiffResult result;
            using (VcEncoder encoder = new(oldDataStream, newDataStream, diffOutput))
                result = await encoder.EncodeAsync();
            Trace.Assert(result == VCDiffResult.SUCCESS);

            output = diffOutput.ToArray();
        }

        return output;
    }

    private static bool StreamContentsEqual(Stream s1, Stream s2, bool resetPos = true)
    {
        s1.Position = 0;
        s2.Position = 0;

        if (s1.Length != s2.Length)
            return false;

        int first;
        int second;
        while (true)
        {
            first = s1.ReadByte();
            second = s2.ReadByte();
            if (first == -1)
            {
                Trace.Assert(second == -1);
                break;
            }

            if (first != second)
                return false;
        }

        if (resetPos)
        {
            s1.Position = 0;
            s2.Position = 0;
        }

        return true;
    }
}
