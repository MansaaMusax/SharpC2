using Shared.Models;
using Shared.Utilities;
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CryptoTest
{
    public class Bob
    {
        string Public;
        string Private;

        string AliceKey;
        byte[] SessionKey;
        byte[] AliceChallenge;

        public Bob()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                Public = rsa.ToXmlString(false);
                Private = rsa.ToXmlString(true);
            }
        }

        public C2Data Stage0Response(C2Data c2data)
        {
            AliceKey = Encoding.UTF8.GetString(c2data.Data);

            return new C2Data
            {
                Module = "Core",
                Command = "Stage0Response",
                Data = Encoding.UTF8.GetBytes(Public)
            };
        }

        public byte[] Stage1Response(byte[] encrypted)
        {
            C2Data c2Data;

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(Private);
                var decrypted = rsa.Decrypt(encrypted, false);
                c2Data = Utilities.DeserialiseData<C2Data>(decrypted);
            }

            SessionKey = Utilities.GetRandomData(32);
            AliceChallenge = Utilities.GetRandomData(16);

            var result = new byte[SessionKey.Length + AliceChallenge.Length];
            Buffer.BlockCopy(SessionKey, 0, result, 0, SessionKey.Length);
            Buffer.BlockCopy(AliceChallenge, 0, result, SessionKey.Length, AliceChallenge.Length);

            var reply = Utilities.SerialiseData(new C2Data
            {
                Module = "Core",
                Command = "Stage1Response",
                Data = result
            });

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(AliceKey);
                return rsa.Encrypt(reply, false);
            }
        }

        public void Stage2Response(byte[] encrypted, byte[] iv)
        {
            var data = Utilities.DeserialiseData<C2Data>(
                Utilities.DecryptData(encrypted, SessionKey, iv));

            if (data.Data.SequenceEqual(AliceChallenge))
            {
                var x = "hurray";
            }
            else
            {
                var x = "boo";
            }
        }
    }
}