using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Files;
public class OpenInstruction : PathInstruction
{
    public OpenInstruction(byte[] body) : base(InstructionType.Open, body)
    {
    }

    public OpenInstruction(string path) : base(InstructionType.Open, path)
    {
    }
}
