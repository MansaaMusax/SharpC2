using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace Shared.Utilities
{
    public class Utilities
    {
        const int BufferSize = 64 * 1024; // 64kB

        public static string GetRandomString(int Length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, Length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static byte[] SerialiseData<T>(T Data)
        {
            using (var ms = new MemoryStream())
            {
                var serialiser = new DataContractJsonSerializer(typeof(T));
                serialiser.WriteObject(ms, Data);
                return Compress(ms.ToArray());
            }
        }

        public static T DeserialiseData<T>(byte[] Data, bool Compressed = true)
        {
            byte[] data;

            if (Compressed)
            {
                data = Decompress(Data);
            }
            else
            {
                data = Data;
            }

            using (var ms = new MemoryStream(data))
            {
                try
                {
                    var serialiser = new DataContractJsonSerializer(typeof(T));
                    return (T)serialiser.ReadObject(ms);
                }
                catch
                {
                    return default;
                }
            }
        }

        public static byte[] Compress(byte[] Data)
        {
            using (var ms = new MemoryStream())
            {
                using (var gzs = new BufferedStream(new GZipStream(ms, CompressionMode.Compress), BufferSize))
                {
                    gzs.Write(Data, 0, Data.Length);
                }

                return ms.ToArray();
            }
        }

        static byte[] Decompress(byte[] Data)
        {
            using (var compressedMs = new MemoryStream(Data))
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