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

namespace NALDownload.Instructions;
internal static class InstructionGenerator
{
    public static async IAsyncEnumerable<Instruction> EnumerateGeneratedDirectoryInstructionsAsync(string oldVersionDirectoryPath, string newVersionDirectoryPath, string currentDirectoryRelativePath, Action<string>? onProgressUpdate = null)
    {
        string NewFullPath(string relPath) => Path.Combine(newVersionDirectoryPath, relPath);
        string OldFullPath(string relPath) => Path.Combine(oldVersionDirectoryPath, relPath);

        DirectoryInfo oldDir = new(OldFullPath(currentDirectoryRelativePath));
        DirectoryInfo newDir = new(NewFullPath(currentDirectoryRelativePath));

        #region Subdirectories
        onProgressUpdate?.Invoke($"Scanning subdirectories of directory '{currentDirectoryRelativePath}'...");
        IEnumerable<string> oldPathSubdirs = oldDir.EnumerateDirectories().Select(di => di.Name);
        IEnumerable<string> newPathSubdirs = newDir.EnumerateDirectories().Select(di => di.Name);

        string[] subDirs = oldPathSubdirs.Concat(newPathSubdirs).Distinct().ToArray();
        // Force case insensitive paths. Path casing is still taken into account in the current spec.
        Trace.Assert(subDirs.DistinctBy(p => p.ToLowerInvariant()).Count() == subDirs.Length);

        foreach (string dirname in subDirs)
        {
            string dirpath = Path.Combine(currentDirectoryRelativePath, dirname);

            if (!Directory.Exists(NewFullPath(dirpath)))
            {
                Trace.Assert(Directory.Exists(OldFullPath(dirpath)));
                yield return new DeleteDirectoryInstruction(dirname);
                continue;
            }

            if (!Directory.Exists(OldFullPath(dirpath)))
            {
                Trace.Assert(Directory.Exists(NewFullPath(dirpath)));
                yield return new CreateDirectoryInstruction(dirname);
            }
            else
            {
                yield return new StepInInstruction(dirname);
            }

            await foreach (Instruction i in EnumerateGeneratedDirectoryInstructionsAsync(oldVersionDirectoryPath, newVersionDirectoryPath, dirpath))
                yield return i;
            yield return new StepOutInstruction();
        }
        #endregion
        #region Files
        onProgressUpdate?.Invoke($"Scanning files of directory '{currentDirectoryRelativePath}'...");
        IEnumerable<string> oldPathFiles = oldDir.EnumerateFiles().Select(fi => fi.Name);
        IEnumerable<string> newPathFiles = newDir.EnumerateFiles().Select(fi => fi.Name);

        string[] files = oldPathFiles.Concat(newPathFiles).Distinct().ToArray();
        Trace.Assert(files.DistinctBy(p => p.ToLowerInvariant()).Count() == files.Length);

        foreach (string filename in files)
        {
            string filepath = Path.Combine(currentDirectoryRelativePath, filename);

            if (!File.Exists(NewFullPath(filepath)))
            {
                Trace.Assert(Directory.Exists(OldFullPath(filepath)));
                yield return new DeleteFileInstruction(filename);
                continue;
            }

            if (File.Exists(OldFullPath(filepath)))
            {
                yield return new OpenInstruction(filename);
                byte[] writeBytes;
                using (FileStream newData = new(NewFullPath(filepath), FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using FileStream oldData = new(OldFullPath(filepath), FileMode.Open, FileAccess.Read, FileShare.Read);
                    writeBytes = await CreateVCDiffAsync(oldData, newData);
                }
                yield return new WriteVcdiff(writeBytes);
            }
            else
            {
                Trace.Assert(File.Exists(NewFullPath(filepath)));
                yield return new CreateFileInstruction(filename);
                byte[] writeBytes;
                using (FileStream newData = new(NewFullPath(filepath), FileMode.Open, FileAccess.Read, FileShare.Read))
                    writeBytes = await CompressToGzipAsync(newData);
                yield return new WriteGzipInstruction(writeBytes);
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
            using (VcEncoder encoder = new(oldDataStream, newDataStream, diffOutput))
                await encoder.EncodeAsync();

            output = diffOutput.ToArray();
        }

        return output;
    }
}
