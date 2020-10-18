using System;
using System.Collections.Generic;

[Serializable]
public class AgentModule
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Developer> Developers { get; set; }
    public List<AgentCommand> Commands { get; set; }
}