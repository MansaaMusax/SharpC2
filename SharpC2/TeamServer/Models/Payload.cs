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

        public byte[] GenerateStager(StagerRequest Request)
        {
            byte[] finalStager = null;

            switch (Listener.Type)
            {
                case Listener.ListenerType.HTTP:
                    finalStager = GenerateHTTPStager(Request);
                    break;

                case Listener.ListenerType.TCP:
                    finalStager = GenerateTCPStager(Request);
                    break;

                case Listener.ListenerType.SMB:
                    finalStager = GenerateSMBStager(Request);
                    break;

                default:
                    break;
            }

            return finalStager;
        }

        byte[] GenerateHTTPStager(StagerRequest Request)
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

            if (Request.SleepInterval == 0)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_0;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 1)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_1;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 2)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_2;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 3)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_3;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 4)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_4;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 5)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_5;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 6)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_6;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 7)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_7;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepInterval == 8)
            {
                sleepInterval.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_8;
                sleepInterval.Body.Instructions[1].Operand = null;
            }
            else
            {
                sleepInterval.Body.Instructions[1].Operand = Request.SleepInterval;
            }

            var sleepJitter = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.Int32 Stager.Stager::get_SleepJitter()", StringComparison.OrdinalIgnoreCase));

            if (Request.SleepJitter == 0)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_0;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 1)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_1;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 2)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_2;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 3)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_3;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 4)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_4;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 5)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_5;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 6)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_6;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 7)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_7;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else if (Request.SleepJitter == 8)
            {
                sleepJitter.Body.Instructions[1].OpCode = OpCodes.Ldc_I4_8;
                sleepJitter.Body.Instructions[1].Operand = null;
            }
            else
            {
                sleepInterval.Body.Instructions[1].Operand = Request.SleepInterval;
            }

            var killDate = stager.Methods.FirstOrDefault(m => m.FullName.Equals("System.DateTime Stager.Stager::get_KillDate()", StringComparison.OrdinalIgnoreCase));
            killDate.Body.Instructions[1].Operand = Request.KillDate.ToString();

            return WriteModule(md);
        }

        byte[] GenerateTCPStager(StagerRequest Request)
        {
            var stager = Helpers.GetEmbeddedResource("stager_tcp.exe");
            var md = ModuleDefMD.Load(stager);

            var stagerType = md.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));

            return WriteModule(md);
        }

        byte[] GenerateSMBStager(StagerRequest Request)
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