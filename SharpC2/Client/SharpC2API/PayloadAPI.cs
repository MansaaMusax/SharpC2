using Newtonsoft.Json;
using RestSharp;

using SharpC2.Models;

using System;
using System.Threading.Tasks;

namespace Client.SharpC2API
{
    public class PayloadAPI
    {
        public static async Task<byte[]> GenerateAgentPayload(PayloadRequest payloadReq)
        {
            var apiRequest = new RestRequest("/api/Payload", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(payloadReq), ParameterType.RequestBody);
            var apiResponse = await REST.Client.ExecuteAsync(apiRequest);

            if (apiResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return Convert.FromBase64String(apiResponse.Content.Replace("\"", ""));
            }
            else
            {
                return new byte[] { };
            }   
        }
    }
}