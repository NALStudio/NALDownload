using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Files;
internal class VerifyFileInstruction : Instruction
{
    public VerifyFileInstruction(byte[] hash) : base(InstructionType.VerifyFileHash, hash)
    {
    }
}
