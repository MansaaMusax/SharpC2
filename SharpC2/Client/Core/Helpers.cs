using System.IO;
using System.IO.Compression;

namespace Client.Core
{
    public class Helpers
    {
        private static int BufferSize = 64 * 1024; // 64kB

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
    }
}