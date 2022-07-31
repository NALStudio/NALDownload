using NALDownload.Instructions.Directories;
using NALDownload.Instructions.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions;
internal static class VerificationGenerator
{
    public static async IAsyncEnumerable<Instruction> EnumerateGeneratedDirectoryVerificationsAsync(string directoryPath, string currentDirectoryRelativePath, Action<string>? onProgressUpdate)
    {
        string fullpath = Path.Combine(directoryPath, currentDirectoryRelativePath);

        onProgressUpdate?.Invoke($"Scanning subdirectories of directory '{currentDirectoryRelativePath}'...");
        foreach (string dir in Directory.GetDirectories(fullpath))
        {
            string subdirFull = Path.Combine(directoryPath, currentDirectoryRelativePath, dir);

            if (Directory.GetFileSystemEntries(subdirFull).Length == 0)
            {
                yield return new VerifyDirectoryInstruction(dir);
                continue;
            }

            yield return new StepInInstruction(dir);
            await foreach (Instruction i in EnumerateGeneratedDirectoryVerificationsAsync(dir, Path.Combine(currentDirectoryRelativePath, dir), onProgressUpdate))
                yield return i;
            yield return new StepOutInstruction();
        }

        onProgressUpdate?.Invoke($"Scanning files of directory '{currentDirectoryRelativePath}'...");
        foreach (string file in Directory.GetFiles(fullpath))
        {
            byte[] hash;
            using (FileStream fs = new(Path.Combine(fullpath, file), FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using MD5 md5 = MD5.Create();
                hash = await md5.ComputeHashAsync(fs);
            }

            yield return new OpenInstruction(file);
            yield return new VerifyFileInstruction(hash);
            yield return new CloseInstruction();
        }
    }
}
