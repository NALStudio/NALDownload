using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Directories;
public class DeleteDirectoryInstruction : PathInstruction
{
    public DeleteDirectoryInstruction(byte[] body) : base(InstructionType.DeleteDirectory, body)
    {
    }

    public DeleteDirectoryInstruction(string path) : base(InstructionType.DeleteDirectory, path)
    {
    }
}
