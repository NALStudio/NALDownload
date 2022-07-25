using NALDownload.Helpers;
using NALDownload.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.NALUpdate;
public static class NALUpdateSerializer
{
    public static byte[] Serialize(NALUpdateDocument document)
    {
        List<byte> bytes = new();

        #region Header
        byte[] header = BitHelpers.GetBytesBigEndian(document.Options.SpecVersion);
        bytes.AddRange(header);
        #endregion

        #region Body
        foreach (Instruction ins in document.Instructions)
        {
            byte[] instructionBytes = ins.ToEscapedByteInstruction(document.Options);

            bytes.Add(Constants.NextInstruction);
            bytes.AddRange(instructionBytes);
        }
        #endregion

        return bytes.ToArray();
    }

    public static NALUpdateDocument Deserialize(byte[] documentBytes)
    {
        int headerSeparatorIndex = Array.FindIndex(documentBytes, b => b == Constants.NextInstruction);
        ThrowInvalidDocIf(headerSeparatorIndex == -1);

        byte[] header = documentBytes[..headerSeparatorIndex];
        byte[] body = documentBytes[(headerSeparatorIndex + 1)..];

        #region Header
        ThrowInvalidDocIf(header.Length < 2);
        NALUpdateDocumentOptions options = new()
        {
            SpecVersion = BitHelpers.ToUInt16BigEndian(header.AsSpan(0, 2))
        };
        #endregion

        #region Body
        List<Instruction> instructions = new();
        foreach (byte[] escapedInstructionBytes in SplitBodyInstructions(body))
        {
            Instruction instruction = Instruction.FromEscapedByteInstruction(escapedInstructionBytes, options);
            instructions.Add(instruction);
        }
        #endregion

        return new(options, instructions);
    }

    private static void ThrowInvalidDocIf(bool condition)
    {
        if (condition)
            throw new ArgumentException("Invalid document bytes.");
    }

    private static IEnumerable<byte[]> SplitBodyInstructions(byte[] body)
    {
        List<byte> cache = new();

        for (int i = 0; i < body.Length; i++)
        {
            byte b1 = body[i];

            int b2Index = i + 1;
            byte? b2 = b2Index < body.Length ? body[b2Index] : null;    

            // (NextInstruction, !NextInstruction) => Split
            if (b1 == Constants.NextInstruction)
            {
                if (b2 == Constants.NextInstruction)
                {
                    cache.Add(Constants.NextInstruction);
                    cache.Add(Constants.NextInstruction);
                    i++;
                }
                else
                {
                    yield return cache.ToArray();
                    cache.Clear();
                }
            }
            else
            {
                cache.Add(b1);
            }
        }

        if (cache.Count > 0)
            yield return cache.ToArray();
    }
}
