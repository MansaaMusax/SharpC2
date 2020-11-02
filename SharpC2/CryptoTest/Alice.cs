using Shared.Models;
using Shared.Utilities;
using System.Runtime.ConstrainedExecution;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTest
{
    public class Alice
    {
        string Public;
        string Private;

        string ServerKey;

        public Alice()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                Public = rsa.ToXmlString(false);
                Private = rsa.ToXmlString(true);
            }
        }

        public C2Data Stage0Request()
        {
            return new C2Data
            {
                Module = "Core",
                Command = "Stage0Request",
                Data = Encoding.UTF8.GetBytes(Public)
            };
        }

        public byte[] Stage1Request(C2Data c2data)
        {
            ServerKey = Encoding.UTF8.GetString(c2data.Data);

            var data = Utilities.SerialiseData(new C2Data
            {
                Module = "Core",
                Command = "Stage1Request"
            });

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(ServerKey);
                return rsa.Encrypt(data, false);
            }
        }

        public byte[] Stage2Request(byte[] encrypted, out byte[] iv)
        {
            C2Data c2data;

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(Private);

                c2data = Utilities.DeserialiseData<C2Data>(
                    rsa.Decrypt(encrypted, false));
            }

            var sessionKey = c2data.Data[0..32];
            var challenge = c2data.Data[32..48];

            var reply = Utilities.SerialiseData(new C2Data
            {
                Module = "Core",
                Command = "Stage2Request",
                Data = challenge
            });

            return Utilities.EncryptData(reply, sessionKey, out iv);
        }
    }
}