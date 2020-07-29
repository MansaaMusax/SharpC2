using Newtonsoft.Json;
using RestSharp;

using SharpC2.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.SharpC2API
{
    public class ServerAPI
    {
        public static async Task<List<ServerEvent>> GetServerEvents()
        {
            var apiRequest = new RestRequest("/api/Server/events", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<ServerEvent>>(apiResponse.Content);
        }
    }
}