using Client.ViewModels;
using Client.Views;

using Newtonsoft.Json;
using RestSharp;

using Shared.Models;

using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Client.API
{
    public class ClientAPI
    {
        static byte[] AcceptedHash;

        public static AuthResult ClientLogin(string host, string port, string nick, string password)
        {
            REST.Client.BaseUrl = new Uri($"https://{host}:{port}");
            REST.Client.AddDefaultHeader("Content-Type", "application/json");

            ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertficate;

            var apiRequest = new RestRequest("/api/users", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(new AuthRequest { Nick = nick, Password = password }), ParameterType.RequestBody);

            var apiResponse = REST.Client.Execute(apiRequest);
            var result = JsonConvert.DeserializeObject<AuthResult>(apiResponse.Content);

            if (result != null && result.Status == AuthResult.AuthStatus.LogonSuccess)
            {
                REST.Client.AddDefaultHeader("Authorization", $"Bearer {result.Token}");
            }

            return result;
        }

        private static bool ValidateServerCertficate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            var thumbprint = cert.GetCertHash();

            if (AcceptedHash!= null && AcceptedHash.SequenceEqual(thumbprint))
            {
                return true;
            }

            var certhashView = new CertThumbprintView();
            var certhashViewModel = new CertThumprintViewModel(certhashView)
            {
                CertHash = BitConverter.ToString(thumbprint)
            };
            certhashView.DataContext = certhashViewModel;

            certhashView.ShowDialog();

            if (certhashViewModel.Accept)
            {
                AcceptedHash = thumbprint;
            }

            return certhashViewModel.Accept;
        }

        public static void ClientLogoff()
        {
            var apiRequest = new RestRequest("/api/Client", Method.DELETE);
            REST.Client.Execute(apiRequest);
        }
    }
}