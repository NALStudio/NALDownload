using NALDownload.Helpers;
using NALDownload.Instructions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.NALUpdate;
public class NALUpdateDocument
{
    public NALUpdateDocumentOptions Options { get; }
    public IEnumerable<Instruction> Instructions { get; }

    public NALUpdateDocument(IEnumerable<Instruction> instructions) : this(null, instructions)
    {
    }

    public NALUpdateDocument(NALUpdateDocumentOptions? options, IEnumerable<Instruction> instructions)
    {
        Options = options ?? new NALUpdateDocumentOptions();
        Instructions = new List<Instruction>(instructions);
    }
}
