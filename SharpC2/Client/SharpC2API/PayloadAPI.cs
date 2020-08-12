using Newtonsoft.Json;
using RestSharp;

using SharpC2.Models;

using System;
using System.Threading.Tasks;

namespace Client.SharpC2API
{
    public class PayloadAPI
    {
        public static async Task<byte[]> GenerateHttpStager(HttpPayloadRequest req)
        {
            var apiRequest = new RestRequest("/api/Payload/http", Method.POST);
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

        public static async Task<byte[]> GenerateTcpStager(TcpPayloadRequest req)
        {
            var apiRequest = new RestRequest("/api/Payload/tcp", Method.POST);
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

        public static async Task<byte[]> GenerateSmbStager(SmbPayloadRequest req)
        {
            var apiRequest = new RestRequest("/api/Payload/smb", Method.POST);
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