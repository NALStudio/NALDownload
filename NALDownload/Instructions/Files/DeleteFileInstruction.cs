using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Files;
public class DeleteFileInstruction : PathInstruction
{
    public DeleteFileInstruction(byte[] body) : base(InstructionType.DeleteFile, body)
    {
    }

    public DeleteFileInstruction(string path) : base(InstructionType.DeleteFile, path)
    {
    }
}
