using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NALDownload.Tester.FileChecking;
internal static class FileChecker
{
    public static async Task CheckDirFiles()
    {
        string dir1 = TestingHelper.GetStringWithCheck("Input directory 1:", p => Directory.Exists(p));
        string dir2 = TestingHelper.GetStringWithCheck("Input directory 2:", p => Directory.Exists(p));

        DirectoryInfo d1 = new(dir1);
        DirectoryInfo d2 = new(dir2);

        FileInfo[] files1 = d1.GetFiles("*", SearchOption.AllDirectories);
        FileInfo[] files2 = d2.GetFiles("*", SearchOption.AllDirectories);

        IEnumerable<(FileInfo First, FileInfo Second)> filesBundle = files1.Zip(files2);
        (FileComparisonData F1, FileComparisonData F2)[] comparisons = filesBundle.Select(f => (new FileComparisonData(f.First), new FileComparisonData(f.Second))).ToArray();

        ConcurrentBag<(FileComparisonData F1, FileComparisonData F2)> notMatching = new();
        int checkedCount = 0;
        Task[] tasks = new Task[comparisons.Length];
        for (int i = 0; i < comparisons.Length; i++)
        {
            (FileComparisonData, FileComparisonData) data = comparisons[i];
            tasks[i] = Task.Run(() => RunComparison(data, ref checkedCount, comparisons.Length, notMatching));
        }

        await Task.WhenAll(tasks);

        foreach ((FileComparisonData f1, FileComparisonData f2) in notMatching)
            TestingHelper.WriteError(string.Join('\n', FileComparisonData.GenerateDiff(f1, f2)));

        Console.WriteLine("Press P to continue.");
        while (Console.ReadKey().Key != ConsoleKey.P) ;
        Console.Clear();
    }

    private static void RunComparison((FileComparisonData, FileComparisonData) data, ref int checkedCount, int totalCount, ConcurrentBag<(FileComparisonData, FileComparisonData)> notMatching)
    {
        FileComparisonData c1 = (FileComparisonData)data.Item1.Clone();
        FileComparisonData c2 = (FileComparisonData)data.Item2.Clone();

        if (c1 != c2)
            notMatching.Add((c1, c2));

        int incremented = Interlocked.Increment(ref checkedCount);
        Console.WriteLine($"Checked file '{c1.Name}'... [{incremented} / {totalCount} ({(float)incremented / totalCount * 100:.00} %)]");
    }
}
