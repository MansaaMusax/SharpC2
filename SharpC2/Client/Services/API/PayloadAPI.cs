using Newtonsoft.Json;
using RestSharp;

using Shared.Models;

using System;
using System.Threading.Tasks;

namespace Client.API
{
    public class PayloadAPI
    {
        public static async Task<byte[]> GenerateStager(StagerRequest req)
        {
            var apiRequest = new RestRequest("/api/Payload/stager", Method.POST);
            apiRequest.AddParameter("application/json", JsonConvert.SerializeObject(req), ParameterType.RequestBody);
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