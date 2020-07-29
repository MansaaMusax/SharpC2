using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TeamServer.Tests
{
    class Helpers
    {
        public static HttpContent Serialise(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        public static T Deserialise<T>(Task<string> data)
        {
            return JsonConvert.DeserializeObject<T>(data.Result);
        }
    }
}