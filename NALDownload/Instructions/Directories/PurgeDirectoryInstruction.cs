using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Directories;
internal class PurgeDirectoryInstruction : PathInstruction
{
    public PurgeDirectoryInstruction(byte[] body) : base(InstructionType.PurgeDirectory, body)
    {
    }

    public PurgeDirectoryInstruction(string path) : base(InstructionType.PurgeDirectory, path)
    {
    }
}
