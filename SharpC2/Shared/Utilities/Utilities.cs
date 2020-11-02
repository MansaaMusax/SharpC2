using Shared.Models;

using System.IO;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;

namespace Shared.Utilities
{
    public class Utilities
    {
        public static byte[] GetRandomData(int Length)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var data = new byte[Length];
                rng.GetNonZeroBytes(data);
                return data;
            }
        }

        public static byte[] EncryptData(C2Data C2Data, byte[] Key, out byte[] IV)
        {
            var data = SerialiseData(C2Data);

            using (var aes = Aes.Create())
            {
                aes.Mode = CipherMode.CBC;
                aes.Key = Key;
                aes.GenerateIV();

                using (var cryptoTransform = aes.CreateEncryptor())
                {
                    IV = aes.IV;
                    return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
                }
            }
        }

        public static T DecryptData<T>(byte[] Data, byte[] Key, byte[] IV)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = Key;
                aes.IV = IV;
                aes.Mode = CipherMode.CBC;

                using (var cryptoTransform = aes.CreateDecryptor())
                {
                    var decrypted = cryptoTransform.TransformFinalBlock(Data, 0, Data.Length);
                    return DeserialiseData<T>(decrypted);
                }
            }
        }

        public static byte[] SerialiseData<T>(T data)
        {
            using (var ms = new MemoryStream())
            {
                var serialiser = new DataContractJsonSerializer(typeof(T));
                serialiser.WriteObject(ms, data);
                return ms.ToArray();
            }
        }

        public static T DeserialiseData<T>(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var serialiser = new DataContractJsonSerializer(typeof(T));
                return (T)serialiser.ReadObject(ms);
            }
        }
    }
}