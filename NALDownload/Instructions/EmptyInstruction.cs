using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions;
public abstract class EmptyInstruction : Instruction
{
    // NO BODY

    private protected EmptyInstruction(InstructionType type) : base(type, Array.Empty<byte>())
    {
    }
}
