using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NALDownload.Helpers;
internal static class GZipHelper
{
    public static async Task CompressToGzipAsync(Stream inputStream, Stream outputStream)
    {
        using GZipStream compressor = new(outputStream, CompressionLevel.SmallestSize, true);
        await inputStream.CopyToAsync(compressor);
    }

    public static async Task DecompressFromGzipAsync(Stream inputStream, Stream outputStream)
    {
        using GZipStream decompressor = new(inputStream, CompressionMode.Decompress);
        await decompressor.CopyToAsync(outputStream);
    }
}
