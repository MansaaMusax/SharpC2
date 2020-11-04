using Client.ViewModels;
using Client.Views;

using Newtonsoft.Json;
using RestSharp;

using Shared.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services
{
    public class SharpC2API
    {
        static byte[] AcceptedHash;

        public static RestClient Client { get; set; } = new RestClient();

        public static bool ValidateServerCertficate(object Sender, X509Certificate Cert, X509Chain Chain, SslPolicyErrors SslPolicyErrors)
        {
            if (SslPolicyErrors == SslPolicyErrors.None)
            {
                return true;
            }

            var thumbprint = Cert.GetCertHash();

            if (AcceptedHash != null && AcceptedHash.SequenceEqual(thumbprint))
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

        public class Users
        {
            public static AuthResult ClientLogin(string Host, string Port, string Nick, string Pass)
            {
                Client.BaseUrl = new Uri($"https://{Host}:{Port}");
                Client.AddDefaultHeader("Content-Type", "application/json");

                ServicePointManager.ServerCertificateValidationCallback = ValidateServerCertficate;

                var apiRequest = new RestRequest("/api/users", Method.POST);
                apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(new AuthRequest { Nick = Nick, Password = Pass }), ParameterType.RequestBody);

                var apiResponse = Client.Execute(apiRequest);
                var result = JsonConvert.DeserializeObject<AuthResult>(apiResponse.Content);

                if (result != null && result.Status == AuthResult.AuthStatus.LogonSuccess)
                {
                    Client.AddDefaultHeader("Authorization", $"Bearer {result.Token}");
                }

                return result;
            }

            public static void ClientLogoff()
            {
                var apiRequest = new RestRequest("/api/users", Method.DELETE);
                Client.Execute(apiRequest);
            }
        }

        public class Server
        {
            public static async Task<List<ServerEvent>> GetServerEvents()
            {
                var apiRequest = new RestRequest("/api/server/events", Method.GET);
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<List<ServerEvent>>(apiResponse.Content);
            }
        }

        public class Agents
        {
            public static async Task<List<AgentMetadata>> GetAgentData()
            {
                var apiRequest = new RestRequest("/api/agents", Method.GET);
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<List<AgentMetadata>>(apiResponse.Content);
            }

            public static async Task<List<AgentEvent>> GetAgentData(string AgentID)
            {
                var apiRequest = new RestRequest($"/api/agents/events?agentid={AgentID}", Method.GET);
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<List<AgentEvent>>(apiResponse.Content);
            }

            public static async void SubmitAgentCommand(string AgentID, string Module, string Command, string Data = "")
            {
                var cmdRequest = new AgentCommandRequest
                {
                    AgentID = AgentID,
                    Module = Module,
                    Command = Command,
                    Data = Encoding.UTF8.GetBytes(Data)
                };

                var apiRequest = new RestRequest($"/api/Agents/command", Method.POST);
                apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(cmdRequest), ParameterType.RequestBody);
                await Client.ExecuteAsync(apiRequest);
            }

            public static async void ClearCommandQueue(string AgentID)
            {
                var apiRequest = new RestRequest($"/api/Agents/clear?agentId={AgentID}", Method.DELETE);
                await Client.ExecuteAsync(apiRequest);
            }

            public static async void RemoveAgent(string AgentID)
            {
                var apiRequest = new RestRequest($"/api/Agents/remove?agentId={AgentID}", Method.DELETE);
                await Client.ExecuteAsync(apiRequest);
            }
        }

        public class Listeners
        {
            public static async Task<Listener> StartListener(ListenerRequest Request)
            {
                var apiRequest = new RestRequest("/api/Listeners", Method.POST);
                apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(Request), ParameterType.RequestBody);
                var apiResponse = await SharpC2API.Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<Listener>(apiResponse.Content);
            }

            public static async void StopListener(string Name)
            {
                var apiRequest = new RestRequest($"/api/listeners?name={Name}", Method.DELETE);
                await Client.ExecuteAsync(apiRequest);
            }

            public static async Task<List<WebLog>> GetWebLogs()
            {
                var apiRequest = new RestRequest("/api/listeners/weblog", Method.GET);
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<List<WebLog>>(apiResponse.Content);
            }

            public static async Task<List<Listener>> GetAllListeners()
            {
                var result = new List<Listener>();

                var httpListeners = await GetHttpListeners();
                var tcpListeners = await GetTcpListeners();
                var smbListeners = await GetSmbListeners();

                if (httpListeners != null)
                {
                    foreach (var listener in httpListeners)
                    {
                        result.Add(listener);
                    }
                }

                if (tcpListeners != null)
                {
                    foreach (var listener in tcpListeners)
                    {
                        result.Add(listener);
                    }
                }

                if (smbListeners != null)
                {
                    foreach (var listener in smbListeners)
                    {
                        result.Add(listener);
                    }
                }

                return result;
            }

            private static async Task<List<ListenerHTTP>> GetHttpListeners()
            {
                var apiRequest = new RestRequest("/api/listeners?type=http", Method.GET);
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<List<ListenerHTTP>>(apiResponse.Content);
            }

            private static async Task<List<ListenerTCP>> GetTcpListeners()
            {
                var apiRequest = new RestRequest("/api/listeners?type=tcp", Method.GET);
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<List<ListenerTCP>>(apiResponse.Content);
            }

            private static async Task<List<ListenerSMB>> GetSmbListeners()
            {
                var apiRequest = new RestRequest("/api/listeners?type=smb", Method.GET);
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                return JsonConvert.DeserializeObject<List<ListenerSMB>>(apiResponse.Content);
            }
        }

        public class Payloads
        {
            public static async Task<byte[]> GenerateStager(StagerRequest Request)
            {
                var apiRequest = new RestRequest("/api/stager", Method.POST);
                apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(Request), ParameterType.RequestBody);
                
                var apiResponse = await Client.ExecuteAsync(apiRequest);

                if (apiResponse.StatusCode == HttpStatusCode.OK)
                {
                    return Convert.FromBase64String(apiResponse.Content.Replace("\"", ""));
                }
                else
                {
                    return null;
                }
            }
        }
    }
}