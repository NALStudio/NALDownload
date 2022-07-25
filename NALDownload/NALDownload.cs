using NALDownload.Instructions;
using NALDownload.Instructions.Directories;
using NALDownload.Instructions.Files;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VCDiff.Encoders;

namespace NALDownload;
public static class NALDownload
{
    public static async Task<IEnumerable<Instruction>> GenerateUpdateInstructions(string oldVersionDirectoryPath, string newVersionDirectoryPath, Action<string>? onProgressUpdate = null)
    {
        if (!(Directory.Exists(oldVersionDirectoryPath) && Directory.Exists(newVersionDirectoryPath)))
            throw new ArgumentException("Both paths must point into an existing directory");

        List<Instruction> instructions = new();

        // Start enumeration from root
        await foreach (Instruction i in InstructionGenerator.EnumerateGeneratedDirectoryInstructionsAsync(oldVersionDirectoryPath, newVersionDirectoryPath, string.Empty, onProgressUpdate))
            instructions.Add(i);

        return instructions;
    }

    public static async Task ApplyUpdateInstructionsAsync(string oldVersionDirectoryPath, IEnumerable<Instruction> instructions, Action<string>? onProgressUpdate = null)
    {
        await InstructionHandler.ApplyInstructionsOnDirectory(oldVersionDirectoryPath, instructions, onProgressUpdate);
    }
}
