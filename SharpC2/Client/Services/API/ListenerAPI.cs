using Newtonsoft.Json;
using RestSharp;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.API
{
    public class ListenerAPI
    {
        public static async Task<Listener> StartListener(NewListenerRequest req)
        {
            var apiRequest = new RestRequest("/api/Listeners", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(req), ParameterType.RequestBody);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<Listener>(apiResponse.Content);
        }

        public static async void StopListener(string name)
        {
            var apiRequest = new RestRequest($"/api/Listeners/{name}", Method.DELETE);
            await REST.Client.ExecuteAsync(apiRequest);
        }

        public static async Task<List<WebLog>> GetWebLogs()
        {
            var apiRequest = new RestRequest("/api/Listeners/weblogs", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

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

        private static async Task<List<ListenerHttp>> GetHttpListeners()
        {
            var apiRequest = new RestRequest("/api/Listeners/http", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<ListenerHttp>>(apiResponse.Content);
        }

        private static async Task<List<ListenerTcp>> GetTcpListeners()
        {
            var apiRequest = new RestRequest("/api/Listeners/tcp", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<ListenerTcp>>(apiResponse.Content);
        }

        private static async Task<List<ListenerSmb>> GetSmbListeners()
        {
            var apiRequest = new RestRequest("/api/Listeners/smb", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<ListenerSmb>>(apiResponse.Content);
        }
    }
}