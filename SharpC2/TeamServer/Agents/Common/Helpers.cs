using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Common
{
    public static class Helpers
    {
        private static int BufferSize = 64 * 1024; // 64kB

        public static byte[] GeneratePseudoRandomBytes(int length)
        {
            return Encoding.UTF8.GetBytes(GeneratePseudoRandomString(length));
        }

        public static string GeneratePseudoRandomString(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
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
}