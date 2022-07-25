using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Files;
public class WriteGzipInstruction : Instruction
{
    public WriteGzipInstruction(byte[] content) : base(InstructionType.WriteGzip, content)
    {
    }
}
