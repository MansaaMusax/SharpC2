using Newtonsoft.Json;
using RestSharp;

using SharpC2.Listeners;
using SharpC2.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.SharpC2API
{
    public class ListenerAPI
    {
        public static async Task<List<ListenerHttp>> GetHttpListeners()
        {
            var apiRequest = new RestRequest("/api/Listeners/http", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<ListenerHttp>>(apiResponse.Content);
        }

        public static async Task<List<ListenerTcp>> GetTcpListeners()
        {
            var apiRequest = new RestRequest("/api/Listeners/tcp", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<ListenerTcp>>(apiResponse.Content);
        }

        public static async Task<List<ListenerSmb>> GetSmbListeners()
        {
            var apiRequest = new RestRequest("/api/Listeners/smb", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<ListenerSmb>>(apiResponse.Content);
        }

        public static async Task<ListenerHttp> StartHttpListener(NewHttpListenerRequest req)
        {
            var apiRequest = new RestRequest("/api/Listeners/http", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(req), ParameterType.RequestBody);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<ListenerHttp>(apiResponse.Content);
        }

        public static async Task<ListenerTcp> StartTcpListener(NewTcpListenerRequest req)
        {
            var apiRequest = new RestRequest("/api/Listeners/tcp", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(req), ParameterType.RequestBody);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<ListenerTcp>(apiResponse.Content);
        }

        public static async Task<ListenerSmb> StartSmbListener(NewSmbListenerRequest req)
        {
            var apiRequest = new RestRequest("/api/Listeners/smb", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(req), ParameterType.RequestBody);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<ListenerSmb>(apiResponse.Content);
        }

        public static async void StopListener(string listenerId, ListenerType type)
        {
            var apiRequest = new RestRequest($"/api/Listeners/{listenerId}?type={type}", Method.DELETE);
            await REST.Client.ExecuteAsync(apiRequest);
        }

        public static async Task<List<WebLog>> GetWebLogs()
        {
            var apiRequest = new RestRequest("/api/Listeners/weblogs", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<WebLog>>(apiResponse.Content);
        }

        public static async Task<List<ListenerBase>> GetAllListeners()
        {
            var result = new List<ListenerBase>();

            var httpListeners = await GetHttpListeners();
            var tcpListeners = await GetTcpListeners();
            var smbListeners = await GetSmbListeners();

            if (httpListeners != null)
            {
                foreach (var httpListener in httpListeners)
                {
                    result.Add(httpListener);
                }
            }
            
            if (tcpListeners != null)
            {
                foreach (var tcpListener in tcpListeners)
                {
                    result.Add(tcpListener);
                }
            }

            if (smbListeners != null)
            {
                foreach (var smbListener in smbListeners)
                {
                    result.Add(smbListener);
                }
            }

            return result;
        }
    }
}