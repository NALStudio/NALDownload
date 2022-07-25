using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions;
public abstract class PathInstruction : Instruction
{
    // BODY LAYOUT (READ SPEC FOR MORE INFO):
    // 00010111 10100000 11100011 00010110 01000011 INSTRUCTION_SEPARATOR
    // ^ body start directory/file(UTF-8)

    private protected PathInstruction(InstructionType type, byte[] body) : base(type, body)
    {
    }

    private protected PathInstruction(InstructionType type, string path) : base(type, Encoding.UTF8.GetBytes(path))
    {
    }

    public string Path => Encoding.UTF8.GetString(Body);
}
