using TeamServer.Listeners;
using TeamServer.Models;
using Xunit;

namespace TeamServer.Tests.ListenerTests
{
    public class Listener
    {
        public Listener()
        {
            new TestClient();
        }

        [Fact]
        public async void StartHttpListener()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listenerRequest = new NewHttpListenerRequest
            {
                BindPort = 8080,
                ConnectAddress = "127.0.0.1",
                ConnectPort = 8080
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Listeners/http", Helpers.Serialise(listenerRequest));
            var result = Helpers.Deserialise<ListenerHttp>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(ListenerType.HTTP, result.Type);
            Assert.Equal(8080, result.BindPort);
            Assert.Equal("127.0.0.1", result.ConnectAddress);
            Assert.Equal(8080, result.ConnectPort);
        }

        [Fact]
        public async void StartTcpListener()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listenerRequest = new NewTcpListenerRequest
            {
                BindAddress = "0.0.0.0",
                BindPort = 8080,
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Listeners/tcp", Helpers.Serialise(listenerRequest));
            var result = Helpers.Deserialise<ListenerTcp>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(ListenerType.TCP, result.Type);
            Assert.Equal("0.0.0.0", result.BindAddress);
            Assert.Equal(8080, result.BindPort);
        }
    }
}