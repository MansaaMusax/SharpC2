using RestSharp;

namespace Client.SharpC2API
{
    public class REST
    {
        public static RestClient Client { get; set; } = new RestClient();
    }
}