using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Tester;
internal static class TestingHelper
{
    public static void WriteError(string error)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ResetColor();
    }

    public static string GetStringWithCheck(string prompt, Func<string, bool> verification)
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
}
