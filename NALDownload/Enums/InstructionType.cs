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
    VerifyFileHash = 0b00000010,
    VerifyDirectoryExists = 0b00000011,
    Open = 0b00000100,
    CreateFile = 0b00000101,
    Close = 0b00000111,
    StepIn = 0b00001100,
    StepOut = 0b00001101,
    CreateDirectory = 0b00001111,
    DeleteFile = 0b11111100,
    DeleteDirectory = 0b11111110,
    PurgeDirectory = 0b11111111
}
