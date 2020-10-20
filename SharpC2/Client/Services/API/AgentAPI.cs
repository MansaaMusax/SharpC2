using Newtonsoft.Json;
using RestSharp;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Client.API
{
    public class AgentAPI
    {
        public static async Task<List<AgentSessionData>> GetAgentData()
        {
            var apiRequest = new RestRequest("/api/Agents", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<AgentSessionData>>(apiResponse.Content);
        }

        public static async Task<List<AgentEvent>> GetAgentData(string agentId)
        {
            var apiRequest = new RestRequest($"/api/Agents/events?agentId={agentId}", Method.GET);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            return JsonConvert.DeserializeObject<List<AgentEvent>>(apiResponse.Content);
        }

        public static async void SubmitAgentCommand(string agentId, string module, string command, string data = "")
        {
            var cmdRequest = new AgentCommandRequest
            {
                AgentId = agentId,
                Module = module,
                Command = command,
                Data = data
            };

            var apiRequest = new RestRequest($"/api/Agents/command", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(cmdRequest), ParameterType.RequestBody);
            await REST.Client.ExecuteAsync(apiRequest);
        }

        public static async void ClearCommandQueue(string agentId)
        {
            var apiRequest = new RestRequest($"/api/Agents/clear?agentId={agentId}", Method.DELETE);
            await REST.Client.ExecuteAsync(apiRequest);
        }

        public static async void RemoveAgent(string agentId)
        {
            var apiRequest = new RestRequest($"/api/Agents/remove?agentId={agentId}", Method.DELETE);
            await REST.Client.ExecuteAsync(apiRequest);
        }
    }
}