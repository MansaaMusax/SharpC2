using Common;

using System;
using System.Linq;
using System.Text;

namespace AgentCore.Controllers
{
    public class CryptoController
    {
        private byte[] EncryptionKey { get; set; } = Convert.FromBase64String("<<EncKey>>"); // Encoding.UTF8.GetBytes("jO8JTskl6BMQTHsZNZ43gz5xEVXb76Zk");

        public byte[] Encrypt<T>(T data)
        {
            var compressed = Common.Helpers.Compress(Serialisation.SerialiseData(data));

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
            var iv = new byte[16];
            var hmac = new byte[32];
            var enc = new byte[data.Length - iv.Length - hmac.Length];

            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(data, enc.Length, hmac, 0, hmac.Length);
            Buffer.BlockCopy(data, iv.Length, enc, 0, enc.Length);

            var decrypted = Cryptography.Decrypt(enc, EncryptionKey, iv);
            var decompressed = Common.Helpers.Decompress(decrypted);

            return Serialisation.DeserialiseData<T>(decompressed);
        }

        public bool VerifyHMAC(byte[] data)
        {
            var iv = new byte[16];
            var hmac = new byte[32];
            var enc = new byte[data.Length - iv.Length - hmac.Length];

            Buffer.BlockCopy(data, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(data, enc.Length + iv.Length, hmac, 0, hmac.Length);
            Buffer.BlockCopy(data, iv.Length, enc, 0, enc.Length);

            var calculated = Cryptography.ComputeHmac256(EncryptionKey, enc);
            return calculated.SequenceEqual(hmac);
        }
    }
}