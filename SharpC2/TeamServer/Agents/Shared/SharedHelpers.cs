using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

public static class SharedHelpers
{
    private static int BufferSize = 64 * 1024; // 64kB

    public static byte[] GeneratePseudoRandomBytes(int length)
    {
        return Encoding.UTF8.GetBytes(GeneratePseudoRandomString(length));
    }

    public static string GeneratePseudoRandomString(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        return new string(Enumerable.Repeat(chars, length)
          .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static byte[] TrimBytes(this byte[] bytes)
    {
        var index = bytes.Length - 1;
        while (bytes[index] == 0) { index--; }
        byte[] copy = new byte[index + 1];
        Array.Copy(bytes, copy, index + 1);
        return copy;
    }

    public static string ConvertFileLength(long size)
    {
        var result = size.ToString();

        if (size < 1024) { result = $"{size}b"; }
        else if (size > 1024 && size <= 1048576) { result = $"{size / 1024}kb"; }
        else if (size > 1048576 && size <= 1073741824) { result = $"{size / 1048576}mb"; }
        else if (size > 1073741824 && size <= 1099511627776) { result = $"{size / 1073741824}gb"; }
        else if (size > 1099511627776) { result = $"{size / 1099511627776}tb"; }

        return result;
    }

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