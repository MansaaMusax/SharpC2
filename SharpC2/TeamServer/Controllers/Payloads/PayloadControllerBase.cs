using System;
using System.IO;
using System.Reflection;

using SharpC2.Models;
using SharpC2.Listeners;

using Common;

namespace TeamServer.Controllers
{
    public class PayloadControllerBase
    {
        public static string RootDirectory { get; set; } = Assembly.GetExecutingAssembly().Location.Split("TeamServer")[0];
        public static string AgentDirectory { get; set; } = RootDirectory + "TeamServer" + Path.DirectorySeparatorChar + "Agents";
        public static string ReferencesDirectory { get; set; } = AgentDirectory + Path.DirectorySeparatorChar + "References";

        public static string GenerateAgentPayload(PayloadRequest request)
        {
            var result = default(string);

            var listener = GetListener(request.ListenerId);
            if (listener == null)
            {
                return result;
            }

            var payload = default(byte[]);
            try
            {
                if (listener.Type == ListenerType.HTTP)
                {
                    var controller = new HttpPayloadController(listener as ListenerHttp);
                    payload = controller.GenerateHttpPayload(request);
                }
                else if (listener.Type == ListenerType.TCP)
                {
                    var controller = new TcpPayloadController(listener as ListenerTcp);
                    payload = controller.GenerateTcpPayload(request);
                }
            }
            catch
            {
                return result;
            }

            return Convert.ToBase64String(payload);
        }

        private static ListenerBase GetListener(string ListenerId)
        {
            return Program.ServerController.ListenerController.GetListener(ListenerId);
        }

        protected static string CreateTempDirectory()
        {
            var temp = Path.GetTempPath() + Helpers.GeneratePseudoRandomString(6);
            Directory.CreateDirectory(temp);
            return temp;
        }

        protected static void CloneAgentSourceCode(ListenerType listenerType, string tempPath)
        {
            var srcPath = default(string);
            switch (listenerType)
            {
                case ListenerType.HTTP:
                    srcPath = AgentDirectory + Path.DirectorySeparatorChar + "HttpAgent";
                    break;
                case ListenerType.TCP:
                    srcPath = AgentDirectory + Path.DirectorySeparatorChar + "TcpAgent";
                    break;
                default:
                    break;
            }

            // AgentType
            var srcFiles = Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories);
            foreach (var filePath in srcFiles)
            {
                if (filePath.Contains("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase) ||
                    filePath.Contains("AssemblyAttributes.cs", StringComparison.OrdinalIgnoreCase)) { continue; }
                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                File.Copy(filePath, finalPath, true);
            }

            // Agent Core
            srcPath = AgentDirectory + Path.DirectorySeparatorChar + "AgentCore";
            foreach (var filePath in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                
                if (File.Exists(finalPath))
                {
                    fileName = fileName.Insert(fileName.Length - 3, Helpers.GeneratePseudoRandomString(6));
                    finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                }
                
                File.Copy(filePath, finalPath, true);
            }

            // Common
            srcPath = AgentDirectory + Path.DirectorySeparatorChar + "Common";
            foreach (var filePath in Directory.GetFiles(srcPath, "*.cs", SearchOption.AllDirectories))
            {
                var fileName = Path.GetFileName(filePath);
                var finalPath = tempPath + Path.DirectorySeparatorChar + fileName;

                if (File.Exists(finalPath))
                {
                    fileName = fileName.Insert(fileName.Length - 3, Helpers.GeneratePseudoRandomString(6));
                    finalPath = tempPath + Path.DirectorySeparatorChar + fileName;
                }

                File.Copy(filePath, finalPath, true);
            }
        }

        protected static void RemoveTempDirectory(string path)
        {
            Directory.Delete(path, true);
        }
    }
}