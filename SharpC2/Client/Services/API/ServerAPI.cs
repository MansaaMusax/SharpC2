using Newtonsoft.Json;
using RestSharp;

using Shared.Models;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.API
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