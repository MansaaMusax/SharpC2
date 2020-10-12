using System.Security.Cryptography;

class Cryptography
{
    public static byte[] Encrypt(byte[] data, byte[] key, out byte[] iv)
    {
        using (var aes = Aes.Create())
        {
            aes.Mode = CipherMode.CBC;
            aes.Key = key;
            aes.GenerateIV();

            using (var cryptoTransform = aes.CreateEncryptor())
            {
                iv = aes.IV;
                return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
            }
        }
    }

    public static byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            using (var cryptoTransform = aes.CreateDecryptor())
            {
                return cryptoTransform.TransformFinalBlock(data, 0, data.Length);
            }
        }
    }

    public static byte[] GetRandomData(int length)
    {
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            var randomData = new byte[length];
            rngCsp.GetBytes(randomData);
            return randomData;
        }
    }

    public static byte[] ComputeHmac256(byte[] key, byte[] data)
    {
        using (var hmac = new HMACSHA256(key))
        {
            return hmac.ComputeHash(data);
        }
    }
}