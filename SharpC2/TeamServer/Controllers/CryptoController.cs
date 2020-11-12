using Shared.Utilities;

using System;
using System.Security.Cryptography;

namespace TeamServer.Controllers
{
    public class CryptoController
    {
        byte[] EncryptionKey;

        public CryptoController()
        {
            EncryptionKey = Convert.FromBase64String("wp8dXiy2AazVc3efsmK0sSF/D3VhxyUutizEpu6WdU4=");
        }

        public byte[] Encrypt<T>(T Data, out byte[] IV)
        {
            var data = Utilities.SerialiseData(Data);

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = EncryptionKey;
                aes.GenerateIV();

                using (var enc = aes.CreateEncryptor())
                {
                    IV = aes.IV;
                    return enc.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        public T Decrypt<T>(byte[] Data, byte[] IV)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;

                using (var dec = aes.CreateDecryptor())
                {
                    var decrypted = dec.TransformFinalBlock(Data, 0, Data.Length);
                    return Utilities.DeserialiseData<T>(decrypted);
                }
            }
        }

        public string GetEncryptionKey()
        {
            return Convert.ToBase64String(EncryptionKey);
        }
    }
}