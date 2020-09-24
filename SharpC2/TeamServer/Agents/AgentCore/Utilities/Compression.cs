using System.IO;
using System.IO.Compression;

class Compression
{
    static int BufferSize = 64 * 1024; // 64kB

    public static byte[] Compress(byte[] data)
    {
        using (var ms = new MemoryStream())
        {
            using (var gzs = new BufferedStream(new GZipStream(ms, CompressionMode.Compress), BufferSize))
            {
                gzs.Write(data, 0, data.Length);
            }

            return ms.ToArray();
        }
    }

    public static byte[] Decompress(byte[] data)
    {
        using (var compressedMs = new MemoryStream(data))
        {
            using (var decompressedMs = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(compressedMs, CompressionMode.Decompress), BufferSize))
                {
                    gzs.CopyTo(decompressedMs);
                }

                return decompressedMs.ToArray();
            }
        }
    }
}