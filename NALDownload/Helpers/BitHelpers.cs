using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Helpers;
internal static class BitHelpers
{
    public static byte[] GetBytesBigEndian(ulong value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        ReverseIfNecessary(ref bytes);
        return bytes;
    }

    public static byte[] GetBytesBigEndian(ushort value)
    {
        byte[] bytes = BitConverter.GetBytes(value);
        ReverseIfNecessary(ref bytes);
        return bytes;
    }

    public static ulong ToUInt64BigEndian(ReadOnlySpan<byte> bigEndianValue)
    {
        ReverseIfNecessary(ref bigEndianValue);
        return BitConverter.ToUInt64(bigEndianValue);
    }

    public static ushort ToUInt16BigEndian(ReadOnlySpan<byte> bigEndianValue)
    {
        ReverseIfNecessary(ref bigEndianValue);
        return BitConverter.ToUInt16(bigEndianValue);
    }

    private static void ReverseIfNecessary(ref ReadOnlySpan<byte> value)
    {
        if (!BitConverter.IsLittleEndian)
            return;

        byte[] temp = value.ToArray();
        Array.Reverse(temp);
        value = temp;
    }

    private static void ReverseIfNecessary(ref byte[] value)
    {
        if (!BitConverter.IsLittleEndian)
            return;

        Array.Reverse(value);
    }
}
