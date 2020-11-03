using Newtonsoft.Json;
using RestSharp;

using Shared.Models;

using System;
using System.Threading.Tasks;

namespace Client.API
{
    public class ClientAPI
    {
        public static async Task<AuthResult> ClientLogin(string host, string port, string nick, string password)
        {
            REST.Client.BaseUrl = new Uri($"https://{host}:{port}");
            REST.Client.RemoteCertificateValidationCallback = (sender, certificate, chain, SslPolicyErrors) => true;
            REST.Client.AddDefaultHeader("Content-Type", "application/json");

            var apiRequest = new RestRequest("/api/Client", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(new AuthRequest { Nick = nick, Password = password }), ParameterType.RequestBody);

            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);
            var result = JsonConvert.DeserializeObject<AuthResult>(apiResponse.Content);

            if (result.Status == AuthResult.AuthStatus.LogonSuccess)
            {
                REST.Client.AddDefaultHeader("Authorization", $"Bearer {result.Token}");
            }

            return result;
        }

        public static void ClientLogoff()
        {
            var apiRequest = new RestRequest("/api/Client", Method.DELETE);
            REST.Client.Execute(apiRequest);
        }
    }
}