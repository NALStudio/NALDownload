using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Directories;
public class CreateDirectoryInstruction : PathInstruction
{
    public CreateDirectoryInstruction(byte[] body) : base(InstructionType.CreateDirectory, body)
    {
    }

    public CreateDirectoryInstruction(string path) : base(InstructionType.CreateDirectory, path)
    {
    }
}
