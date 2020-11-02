using Shared.Models;

using System.Collections.Generic;

namespace Stager.Comms
{
    public abstract class CommModule
    {
        protected ModuleStatus Status;

        protected Queue<AgentMessage> Inbound = new Queue<AgentMessage>();
        protected Queue<AgentMessage> Outbound = new Queue<AgentMessage>();

        public virtual void SendData(AgentMessage Message)
        {
            Outbound.Enqueue(Message);
        }

        public virtual bool RecvData(out AgentMessage Message)
        {
            if (Inbound.Count > 0)
            {
                Message = Inbound.Dequeue();
                return true;
            }
            else
            {
                Message = null;
                return false;
            }
        }

        public abstract void Start();

        public virtual void Stop()
        {
            Status = ModuleStatus.Stopped;
        }
    }
}