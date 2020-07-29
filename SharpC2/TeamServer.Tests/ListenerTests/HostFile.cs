using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TeamServer.Listeners;
using TeamServer.Models;
using Xunit;

namespace TeamServer.Tests.ListenerTests
{
    public class HostFile
    {
        public HostFile()
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

        [Fact]
        public async void NewHostedFile()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listener = await StartHttpListener();
            var content = "this is a test";
            var hostedFile = new NewHostedFileRequest
            {
                Uri = "/a",
                EncodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
                MimeType = MimeType.Text_Plain
            };

            var apiReq = await TestClient.HttpClient.PostAsync($"api/Listeners/{listener.ListenerId}/hostfile", Helpers.Serialise(hostedFile));
            var result = Helpers.Deserialise<HostedFileResponse>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(hostedFile.Uri, result.Uri);
            Assert.Equal(content.Length, result.DataLength);
            Assert.Equal(hostedFile.MimeType, result.MimeType);
        }

        [Fact]
        public async void OverwriteHostedFile()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listener = await StartHttpListener();
            var content = "this is a test";
            var hostedFile = new NewHostedFileRequest
            {
                Uri = "/a",
                EncodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
                MimeType = MimeType.Text_Plain
            };

            await TestClient.HttpClient.PostAsync($"api/Listeners/{listener.ListenerId}/hostfile", Helpers.Serialise(hostedFile));

            var newContent = "this is a another test";
            var newHostedFile = new NewHostedFileRequest
            {
                Uri = "/a",
                EncodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(newContent)),
                MimeType = MimeType.Text_Plain
            };

            await TestClient.HttpClient.PostAsync($"api/Listeners/{listener.ListenerId}/hostfile", Helpers.Serialise(newHostedFile));

            var apiReq = await TestClient.HttpClient.GetAsync("api/Listeners/hostedfiles");
            var result = Helpers.Deserialise<List<HostedFileResponse>>(apiReq.Content.ReadAsStringAsync());

            Assert.Equal(newHostedFile.Uri, result[0].Uri);
            Assert.Equal(newContent.Length, result[0].DataLength);
            Assert.Equal(newHostedFile.MimeType, result[0].MimeType);
        }

        [Fact]
        public async void RemoveHostedFile()
        {
            await TestClient.ClientLogin(TeamServer.Helpers.GeneratePseudoRandomString(6), "a");

            var listener = await StartHttpListener();
            var content = "this is a test";
            var hostedFile = new NewHostedFileRequest
            {
                Uri = "/a",
                EncodedContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(content)),
                MimeType = MimeType.Text_Plain
            };

            await TestClient.HttpClient.PostAsync($"api/Listeners/{listener.ListenerId}/hostfile", Helpers.Serialise(hostedFile));
            //await TestClient.HttpClient.DeleteAsync

            var apiReq = await TestClient.HttpClient.GetAsync("api/Listeners/hostedfiles");
            var result = Helpers.Deserialise<List<HostedFileResponse>>(apiReq.Content.ReadAsStringAsync());


        }
    }
}