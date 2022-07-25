using NALDownload.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Instructions.Files;
public class WriteVcdiff : Instruction
{
    public WriteVcdiff(byte[] vcdiff) : base(InstructionType.WriteVcdiff, vcdiff)
    {
    }
}
