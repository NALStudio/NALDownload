using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Directories;
public class StepOutInstruction : EmptyInstruction
{
    public StepOutInstruction() : base(InstructionType.StepOut)
    {
    }
}
