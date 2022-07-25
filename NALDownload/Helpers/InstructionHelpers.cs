using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Helpers;
internal static class InstructionHelpers
{
    public static IEnumerable<byte> EscapeBytes(byte[] bytes)
    {
        foreach (byte b in bytes)
        {
            yield return b;
            if (b == Constants.NextInstruction)
                yield return Constants.NextInstruction;
        }
    }

    public static IEnumerable<byte> UnescapeBytes(byte[] bytes)
    {
        for (int i = 0; i < bytes.Length; i++)
        {
            byte b1 = bytes[i];

            int b2Index = i + 1;
            byte? b2 = b2Index < bytes.Length ? bytes[b2Index] : null;

            if (b1 != Constants.NextInstruction)
            {
                yield return b1;
                continue;
            }

            if (b2 != Constants.NextInstruction)
                throw new ArgumentException($"Unhandled instruction split in bytes. Following byte: '{b2}' (null: {b2 is null})", nameof(bytes));

            // (NextInstruction, NextInstruction) => NextInstruction
            yield return Constants.NextInstruction;
            i++; // Skip the second NextInstruction of escape
        }
    }
}
