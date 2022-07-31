using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Exceptions;
internal class VerificationException : Exception
{
    public VerificationException() : base()
    {
    }

    public VerificationException(string? message) : base(message)
    {
    }

    public VerificationException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}
