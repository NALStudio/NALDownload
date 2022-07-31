using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Directories;
internal class VerifyDirectoryInstruction : PathInstruction
{
    public VerifyDirectoryInstruction(byte[] body) : base(InstructionType.VerifyDirectoryExists, body)
    {
    }

    public VerifyDirectoryInstruction(string path) : base(InstructionType.VerifyDirectoryExists, path)
    {
    }
}
