using Common;

using System;
using System.Linq;
using System.Text;

namespace TeamServer.Controllers
{
    public class CryptoController
    {
        public byte[] EncryptionKey { get; private set; } = Cryptography.GetRandomData(32); // Encoding.UTF8.GetBytes("jO8JTskl6BMQTHsZNZ43gz5xEVXb76Zk");

        public byte[] Encrypt<T>(T data)
        {
            var compressed = Helpers.Compress(Serialisation.SerialiseData(data));

            var encryptedData = Cryptography.Encrypt(compressed, EncryptionKey, out byte[] iv);
            var hmac = Cryptography.ComputeHmac256(EncryptionKey, encryptedData);

            var result = new byte[iv.Length + encryptedData.Length + hmac.Length];

            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encryptedData, 0, result, iv.Length, encryptedData.Length);
            Buffer.BlockCopy(hmac, 0, result, iv.Length + encryptedData.Length, hmac.Length);

            return result;
        }

        public T Decrypt<T>(byte[] data)
        {
            var iv = data[0..16];
            var hmac = data[(data.Length - 32)..data.Length];
            var enc = data[iv.Length..(data.Length-hmac.Length)];

            var decrypted = Cryptography.Decrypt(enc, EncryptionKey, iv);
            var decompressed = Helpers.Decompress(decrypted);

            return Serialisation.DeserialiseData<T>(decompressed);
        }

        public bool VerifyHMAC(byte[] data)
        {
            var iv = data[0..16];
            var hmac = data[(data.Length - 32)..data.Length];
            var enc = data[iv.Length..(data.Length - hmac.Length)];

            var calculated = Cryptography.ComputeHmac256(EncryptionKey, enc);
            return calculated.SequenceEqual(hmac);
        }
    }
}