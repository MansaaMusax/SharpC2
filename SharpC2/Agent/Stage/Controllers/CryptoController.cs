using System;
using System.Security.Cryptography;

namespace Agent.Controllers
{
    public class CryptoController
    {
        byte[] EncryptionKey;

        public CryptoController(byte[] EncryptionKey)
        {
            this.EncryptionKey = EncryptionKey;
        }

        public byte[] Encrypt<T>(T Data, out byte[] IV)
        {
            var data = Shared.Utilities.Utilities.SerialiseData(Data);

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
                    return Shared.Utilities.Utilities.DeserialiseData<T>(decrypted);
                }
            }
        }
    }
}