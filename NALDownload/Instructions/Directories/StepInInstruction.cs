using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Directories;
public class StepInInstruction : PathInstruction
{
    public StepInInstruction(byte[] body) : base(InstructionType.StepIn, body)
    {
    }

    public StepInInstruction(string path) : base(InstructionType.StepIn, path)
    {
    }
}
