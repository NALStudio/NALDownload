using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Tester.FileChecking;
internal class FileComparisonData : ICloneable
{
    public string Name { get; }
    public long Length { get; }

    private byte[]? _body = null;
    public byte[] Body => _body ??= File.ReadAllBytes(_fullName);

    private readonly string _fullName;

    public FileComparisonData(FileInfo fi) : this(fi.Name, fi.FullName, fi.Length, null)
    {
    }

    public FileComparisonData(string name, string fullName, long length, byte[]? body)
    {
        Name = name;
        _fullName = fullName;
        Length = length;
        _body = body;
    }

    public static IEnumerable<string> GenerateDiff(FileComparisonData lhs, FileComparisonData rhs)
    {
        static string GetDiffString<T>(bool equal, T lhs, T rhs)
        {
            string symbol = equal ? "==" : "!=";

            return $"'{lhs}' {symbol} '{rhs}'";
        }

        yield return GetDiffString(lhs.Name == rhs.Name, lhs.Name, rhs.Name);
        yield return GetDiffString(lhs.Length == rhs.Length, lhs.Length, rhs.Length);
        yield return GetDiffString(lhs.Body.SequenceEqual(rhs.Body), "{body1}", "{body2}");
    }

    public override bool Equals(object? obj)
    {
        return obj is FileComparisonData fd && Equals(fd);
    }

    public bool Equals(FileComparisonData fd)
    {
        return Name == fd.Name &&
               Length == fd.Length &&
               Body.SequenceEqual(fd.Body);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Length, Body);
    }

    public object Clone()
    {
        return new FileComparisonData(Name, _fullName, Length, _body);
    }

    public static bool operator ==(FileComparisonData? lhs, FileComparisonData? rhs)
    {
        if (lhs is null)
            return rhs is null;
        return lhs.Equals((object?)rhs);
    }

    public static bool operator !=(FileComparisonData? lhs, FileComparisonData? rhs)
    {
        return !(lhs == rhs);
    }
}
