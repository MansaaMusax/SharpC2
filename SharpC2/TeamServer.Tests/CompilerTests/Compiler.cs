using System.Threading.Tasks;
using TeamServer.Listeners;
using TeamServer.Models;
using Xunit;

namespace TeamServer.Tests.CompilerTests
{
    public class Compiler
    {
        public Compiler()
        {
            new TestClient();
        }

        internal async Task<ListenerHttp> StartHttpListener()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listenerRequest = new NewHttpListenerRequest
            {
                BindPort = 8080,
                ConnectAddress = "127.0.0.1",
                ConnectPort = 8080
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Listeners/http", Helpers.Serialise(listenerRequest));
            return Helpers.Deserialise<ListenerHttp>(apiReq.Content.ReadAsStringAsync());
        }

        internal async Task<ListenerTcp> StartTcpListener()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listenerRequest = new NewTcpListenerRequest
            {
                BindAddress = "0.0.0.0",
                BindPort = 8080,
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Listeners/tcp", Helpers.Serialise(listenerRequest));
            return Helpers.Deserialise<ListenerTcp>(apiReq.Content.ReadAsStringAsync());
        }

        [Fact]
        public async void GenerateHttpAgentPayloadSuccess()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listener = await StartHttpListener();

            var payloadRequest = new PayloadRequest
            {
                ListenerId = listener.ListenerId,
                OutputType = OutputType.Exe,
                TargetFramework = TargetFramework.Net40
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Payload", Helpers.Serialise(payloadRequest));
            var result = Helpers.Deserialise<PayloadResponse>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(CompilerStatus.Success, result.CompilerStatus);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.EncodedAssembly);
        }

        [Fact]
        public async void GenerateHttpAgentPayloadFail()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var payloadRequest = new PayloadRequest
            {
                ListenerId = "blah",
                OutputType = OutputType.Exe,
                TargetFramework = TargetFramework.Net40
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Payload", Helpers.Serialise(payloadRequest));
            var result = Helpers.Deserialise<PayloadResponse>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(CompilerStatus.Fail, result.CompilerStatus);
            Assert.Null(result.EncodedAssembly);
            Assert.NotNull(result.ErrorMessage);
        }

        [Fact]
        public async void GenerateTcpAgentPayloadSuccess()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listener = await StartTcpListener();

            var payloadRequest = new PayloadRequest
            {
                ListenerId = listener.ListenerId,
                OutputType = OutputType.Exe,
                TargetFramework = TargetFramework.Net40
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Payload", Helpers.Serialise(payloadRequest));
            var result = Helpers.Deserialise<PayloadResponse>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(CompilerStatus.Success, result.CompilerStatus);
            Assert.Null(result.ErrorMessage);
            Assert.NotNull(result.EncodedAssembly);
        }

        [Fact]
        public async void GenerateTcpAgentPayloadFail()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var payloadRequest = new PayloadRequest
            {
                ListenerId = "blah",
                OutputType = OutputType.Exe,
                TargetFramework = TargetFramework.Net40
            };

            var apiReq = await TestClient.HttpClient.PostAsync("api/Payload", Helpers.Serialise(payloadRequest));
            var result = Helpers.Deserialise<PayloadResponse>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(CompilerStatus.Fail, result.CompilerStatus);
            Assert.Null(result.EncodedAssembly);
            Assert.NotNull(result.ErrorMessage);
        }
    }
}