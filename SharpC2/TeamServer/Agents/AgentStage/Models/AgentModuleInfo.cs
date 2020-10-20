using System;
using System.Collections.Generic;

[Serializable]
public class AgentModuleInfo
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Developer> Developers { get; set; }
    public List<AgentCommand> Commands { get; set; }
    public bool NotifyTeamServer { get; set; } = true;
}

[Serializable]
public class AgentCommand
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string HelpText { get; set; }
    public bool Visible { get; set; } = true;

    [NonSerialized] AgentController.OnAgentCommand _callback;

    public AgentController.OnAgentCommand CallBack
    {
        get { return _callback; }
        set { _callback = value; }
    }
}