using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace TeamServer
{
    public class Helpers
    {
        public static byte[] GetEmbeddedResource(string ResourceName)
        {
            var self = Assembly.GetExecutingAssembly();
            var name = $"TeamServer.Resources.{ResourceName}";

            using (var rs = self.GetManifestResourceStream(name))
            {
                if (rs != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        rs.CopyTo(ms);
                        ms.Position = 0;
                        return ms.ToArray();
                    }
                }
                else
                {
                    return null;
                }
                
            }
        }

        public static byte[] GenerateRandomBytes(int Length)
        {
            var data = new byte[Length];
            
            var rng = RandomNumberGenerator.Create();
            rng.GetNonZeroBytes(data);

            return data;
        }
    }
}