using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Files;
public class CreateFileInstruction : PathInstruction
{
    public CreateFileInstruction(byte[] body) : base(InstructionType.CreateFile, body)
    {
    }

    public CreateFileInstruction(string path) : base(InstructionType.CreateFile, path)
    {
    }
}
