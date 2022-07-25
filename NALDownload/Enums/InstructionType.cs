using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Enums;
public enum InstructionType : byte
{
    WriteGzip = 0b00000000,
    WriteVcdiff = 0b00000001,
    Open = 0b00000010,
    CreateFile = 0b00000011,
    StepIn = 0b00000100,
    StepOut = 0b00000101,
    CreateDirectory = 0b00000111,
    DeleteFile = 0b11111110,
    DeleteDirectory = 0b11111111
}
