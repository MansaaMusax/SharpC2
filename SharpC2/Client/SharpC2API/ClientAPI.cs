using Newtonsoft.Json;
using RestSharp;

using SharpC2.Models;

using System;
using System.Threading.Tasks;

namespace Client.SharpC2API
{
    public class ClientAPI
    {
        public static async Task<ClientAuthenticationResult> ClientLogin(string host, string port, string nick, string password)
        {
            REST.Client.BaseUrl = new Uri($"https://{host}:{port}");
            REST.Client.RemoteCertificateValidationCallback = (sender, certificate, chain, SslPolicyErrors) => true;
            REST.Client.AddDefaultHeader("Content-Type", "application/json");

            var apiRequest = new RestRequest("/api/Client", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(new ClientAuthenticationRequest { Nick = nick, Password = password }), ParameterType.RequestBody);

            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);
            var result = JsonConvert.DeserializeObject<ClientAuthenticationResult>(apiResponse.Content);

            if (result.Result == ClientAuthenticationResult.AuthResult.LoginSuccess)
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