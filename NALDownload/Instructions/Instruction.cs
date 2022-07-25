using NALDownload.Enums;
using NALDownload.Helpers;
using NALDownload.Instructions.Directories;
using NALDownload.Instructions.Files;
using NALDownload.NALUpdate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions;
public abstract class Instruction
{
    private protected Instruction(InstructionType type, byte[] body)
    {
        Type = type;
        Body = body;
    }

    public InstructionType Type { get; }
    public byte[] Body { get; }

    public byte[] ToEscapedByteInstruction(NALUpdateDocumentOptions options)
    {
        // 00000000 (00000000 00000000)
        // ^ type   ^ (optional) body

        byte[] unescapedBody = Body;
        byte[] body = InstructionHelpers.EscapeBytes(unescapedBody).ToArray();

        byte[] output = new byte[body.Length + 1];
        byte byteType = (byte)Type;
        if (byteType == Constants.NextInstruction)
            throw new InvalidOperationException($"Cannot convert type {byteType} into byte instruction due to next instruction mismatch.");
        output[0] = byteType;
        Array.Copy(body, 0, output, 1, body.Length);

        return output;
    }

    public static Instruction FromEscapedByteInstruction(byte[] instruction, NALUpdateDocumentOptions options)
    {
        if (instruction.Length < 1)
            throw new ArgumentException("Invalid instruction!", nameof(instruction));

        InstructionType type = (InstructionType)instruction[0];
        byte[] escapedBody = instruction[1..];
        byte[] body = InstructionHelpers.UnescapeBytes(escapedBody).ToArray();

        return type switch
        {
            InstructionType.WriteGzip => new WriteGzipInstruction(body),
            InstructionType.WriteVcdiff => new WriteVcdiff(body),
            InstructionType.StepIn => new StepInInstruction(body),
            InstructionType.StepOut => new StepOutInstruction(),
            InstructionType.CreateDirectory => new CreateDirectoryInstruction(body),
            InstructionType.Open => new OpenInstruction(body),
            InstructionType.CreateFile => new CreateFileInstruction(body),
            InstructionType.DeleteFile => new DeleteFileInstruction(body),
            InstructionType.DeleteDirectory => new DeleteDirectoryInstruction(body),
            _ => throw new ArgumentException($"Cannot parse type: '{type}' to an instruction type.")
        };
    }
}
