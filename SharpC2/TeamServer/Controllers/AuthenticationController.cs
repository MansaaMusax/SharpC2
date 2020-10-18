using Microsoft.IdentityModel.Tokens;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace TeamServer.Controllers
{
    public class AuthenticationController
    {
        private static byte[] ServerPassword { get; set; }
        public static byte[] JWTSecret { get; private set; } = Helpers.GeneratePseudoRandomBytes(128);

        public static void SetPassword(string plaintext)
        {
            ServerPassword = HashPassword(plaintext);
        }

        public static bool ValidatePassword(string plaintext)
        {
            return ServerPassword.SequenceEqual(HashPassword(plaintext));
        }

        public static string GenerateAuthenticationToken(string nick)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, nick) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(JWTSecret), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private static byte[] HashPassword(string plaintext)
        {
            using (var crypto = SHA512.Create())
            {
                return crypto.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            }
        }
    }
}