using NALDownload.Instructions;
using NALDownload.NALUpdate;
using NALDownload.Tester.FileChecking;
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
        ConsoleKeyInfo key;
        do
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("*** PRESS KEY ***");
            Console.ResetColor();
            Console.WriteLine("A: Apply diff");
            Console.WriteLine("C: Create diff");
            Console.WriteLine("P: Check directory files");

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
                case ConsoleKey.P:
                    await FileChecker.CheckDirFiles();
                    break;
                default:
                    TestingHelper.WriteError($"Invalid key: {key.Key}");
                    break;
            }
        }
        while (key.Key != ConsoleKey.Escape);
    }

    private static async Task CreateDiff()
    {
        string oldDir = TestingHelper.GetStringWithCheck("Input old version directory path:", p => Directory.Exists(p));
        string newDir = TestingHelper.GetStringWithCheck("Input new version directory path:", p => Directory.Exists(p));
        string diffPath = TestingHelper.GetStringWithCheck("Input diff file path:", p => !File.Exists(p));

        IEnumerable<Instruction> instructions = await NALDownload.GenerateUpdateInstructions(oldDir, newDir, update => Console.WriteLine(update));

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        foreach (Instruction i in instructions)
            Console.WriteLine(i.Type + (i is PathInstruction pi ? $" '{pi.Path}'" : string.Empty));
        Console.ResetColor();

        NALUpdateDocument doc = new(instructions);
        byte[] bytes = NALUpdateSerializer.Serialize(doc);
        await File.WriteAllBytesAsync(diffPath, bytes);
    }

    private static async Task ApplyDiff()
    {
        string oldDir = TestingHelper.GetStringWithCheck("Input old version directory path:", p => Directory.Exists(p));
        string diffPath = TestingHelper.GetStringWithCheck("Input diff file path:", p => File.Exists(p));

        NALUpdateDocument doc = NALUpdateSerializer.Deserialize(await File.ReadAllBytesAsync(diffPath));

        await NALDownload.ApplyUpdateInstructionsAsync(oldDir, doc.Instructions, update => Console.WriteLine(update));
    }
}
