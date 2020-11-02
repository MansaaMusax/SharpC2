using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace TeamServer.Controllers
{
    public class UserController
    {
        byte[] ServerPassword;
        byte[] JWTSecret;

        List<string> ConnectedUsers = new List<string>();

        public UserController()
        {
            JWTSecret = Encoding.UTF8.GetBytes("gvcznxrbobuzvvzytfynvvzrpqaaihrvkrgbgnfqdzdwojjzbiymzcfeuywidvjuwdnlvplwlzcwjbyfaveegnvxnvfcbjdwgggywzngsoxxyroaiogmcmvisdmogfge");
        }

        public void SetServerPassword(string Plaintext)
        {
            ServerPassword = HashPassword(Plaintext);
        }

        public bool ValidatePassword(string Plaintext)
        {
            return ServerPassword.SequenceEqual(HashPassword(Plaintext));
        }

        public string GenerateAuthenticationToken(string Nick)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, Nick) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes("gvcznxrbobuzvvzytfynvvzrpqaaihrvkrgbgnfqdzdwojjzbiymzcfeuywidvjuwdnlvplwlzcwjbyfaveegnvxnvfcbjdwgggywzngsoxxyroaiogmcmvisdmogfge")),
                    SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public AuthResult UserLogon(AuthRequest Request)
        {
            var result = new AuthResult();

            if (ValidatePassword(Request.Password))
            {
                if (!ConnectedUsers.Contains(Request.Nick))
                {
                    result.Status = AuthResult.AuthStatus.LogonSuccess;
                    result.Token = GenerateAuthenticationToken(Request.Nick);
                    AddUser(Request.Nick);
                }
                else
                {
                    result.Status = AuthResult.AuthStatus.NickInUse;
                }
            }
            else
            {
                result.Status = AuthResult.AuthStatus.BadPassword;
            }

            return result;
        }

        public void AddUser(string Nick)
        {
            if (!ConnectedUsers.Contains(Nick))
            {
                ConnectedUsers.Add(Nick);
            }
        }

        public IEnumerable<string> GetConnectedUsers()
        {
            return ConnectedUsers;
        }

        public bool RemoveUser(string Nick)
        {
            return ConnectedUsers.Remove(Nick);
        }

        byte[] HashPassword(string Plaintext)
        {
            using (var crypto = SHA256.Create())
            {
                return crypto.ComputeHash(Encoding.UTF8.GetBytes(Plaintext));
            }
        }
    }
}