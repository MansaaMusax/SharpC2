using RestSharp;

namespace Client.API
{
    public class REST
    {
        public static RestClient Client { get; set; } = new RestClient();
    }
}