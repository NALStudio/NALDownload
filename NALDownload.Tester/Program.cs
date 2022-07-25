using NALDownload.Instructions;
using NALDownload.NALUpdate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Tester;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine("MORE TESTING REQUIRED");
        Console.WriteLine("MORE TESTING REQUIRED");
        Console.WriteLine("MORE TESTING REQUIRED");
        Console.WriteLine("MORE TESTING REQUIRED");

        ConsoleKeyInfo key;
        do
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("*** PRESS KEY ***");
            Console.ResetColor();
            Console.WriteLine("A: Apply diff");
            Console.WriteLine("C: Create diff");

            key = Console.ReadKey();
            Console.Clear();
            switch (key.Key)
            {
                case ConsoleKey.C:
                    await CreateDiff();
                    break;
                case ConsoleKey.A:
                    await ApplyDiff();
                    break;
                default:
                    WriteError($"Invalid key: {key.Key}");
                    break;
            }
        }
        while (key.Key != ConsoleKey.Escape);
    }

    private static void WriteError(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ResetColor();
    }

    private static string GetStringWithCheck(string prompt, Func<string, bool> verification)
    {
        Console.Clear();
        string? value;
        do
        {
            Console.WriteLine(prompt);
            value = Console.ReadLine();
            Console.Clear();

            if (value is null)
            {
                WriteError("Out of lines.");
            }
            else if (!verification(value))
            {
                WriteError("Invalid input.");
                value = null;
            }
        }
        while (value is null);

        return value;
    }

    private static async Task CreateDiff()
    {
        string oldDir = GetStringWithCheck("Input old version directory path:", p => Directory.Exists(p));
        string newDir = GetStringWithCheck("Input new version directory path:", p => Directory.Exists(p));
        string diffPath = GetStringWithCheck("Input diff file path:", p => !File.Exists(p));

        IEnumerable<Instruction> instructions = await NALDownload.GenerateUpdateInstructions(oldDir, newDir, update => Console.WriteLine(update));
        NALUpdateDocument doc = new(instructions);
        byte[] bytes = NALUpdateSerializer.Serialize(doc);
        await File.WriteAllBytesAsync(diffPath, bytes);
    }

    private static async Task ApplyDiff()
    {
        string oldDir = GetStringWithCheck("Input old version directory path:", p => Directory.Exists(p));
        string diffPath = GetStringWithCheck("Input diff file path:", p => File.Exists(p));

        NALUpdateDocument doc = NALUpdateSerializer.Deserialize(await File.ReadAllBytesAsync(diffPath));

        await NALDownload.ApplyUpdateInstructionsAsync(oldDir, doc.Instructions, update => Console.WriteLine(update));
    }
}
