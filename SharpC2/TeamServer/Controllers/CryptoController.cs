using Shared.Models;
using Shared.Utilities;

using System.Collections.Generic;
using System.Security.Cryptography;

namespace TeamServer.Controllers
{
    public class CryptoController
    {
        public string PublicKey;
        string PrivateKey;

        Dictionary<string, string> PublicKeys = new Dictionary<string, string>();
        Dictionary<string, byte[]> SessionKeys = new Dictionary<string, byte[]>();
        Dictionary<string, byte[]> Challenges = new Dictionary<string, byte[]>();

        public CryptoController()
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                PublicKey = rsa.ToXmlString(false);
                PrivateKey = rsa.ToXmlString(true);
            }
        }

        public void AddAgentPublicKey(string AgentID, string PublicKey)
        {
            if (!PublicKeys.ContainsKey(AgentID))
            {
                PublicKeys.Add(AgentID, PublicKey);
            }
            else
            {
                PublicKeys[AgentID] = PublicKey;
            }
        }

        public byte[] GenerateSessionKey(string AgentID)
        {
            var random = Utilities.GetRandomData(32);

            if (!SessionKeys.ContainsKey(AgentID))
            {
                SessionKeys.Add(AgentID, random);
            }
            else
            {
                SessionKeys[AgentID] = random;
            }

            return random;
        }

        public byte[] GenerateChallenge(string AgentID)
        {
            var random = Utilities.GetRandomData(8);

            if (!Challenges.ContainsKey(AgentID))
            {
                Challenges.Add(AgentID, random);
            }
            else
            {
                Challenges[AgentID] = random;
            }

            return random;
        }

        public byte[] GetAgentChallenge(string AgentID)
        {
            if (Challenges.ContainsKey(AgentID))
            {
                return Challenges[AgentID];
            }
            else
            {
                return null;
            }
        }

        public byte[] GetSessionKey(string AgentID)
        {
            if (SessionKeys.ContainsKey(AgentID))
            {
                return SessionKeys[AgentID];
            }
            else
            {
                return null;
            }
        }

        public byte[] Encrypt(string AgentID, C2Data C2Data)
        {
            var data = Utilities.SerialiseData(C2Data);

            var agentKey = PublicKeys[AgentID];

            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                rsa.FromXmlString(agentKey);
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