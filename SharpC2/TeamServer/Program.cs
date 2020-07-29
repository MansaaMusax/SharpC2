using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Serilog;

using System;

using TeamServer.Controllers;
using TeamServer.Modules;

namespace TeamServer
{
    public class Program
    {
        public static ServerController ServerController { get; private set; }

        public static void Main(string[] args)
        {
            var pass = args.Length > 0 ? args[0] : string.Empty;
            PrintLogo(); // the most important part
            StartLogger();
            StartTeamServer(pass);
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        public static void StartTeamServer(string pass = "")
        {
            if (string.IsNullOrEmpty(pass))
            {
                pass = PromptForPass();
            }

            AuthenticationController.SetPassword(pass);

            ServerController = new ServerController();
            ServerController.RegisterServerModule(new CoreServerModule());
            ServerController.RegisterServerModule(new ReversePortForwardModule());
            ServerController.Start();

            Log.Logger.Information("SERVER is {ServerStatus}", ServerController.ServerStatus);
        }

        private static void PrintLogo()
        {
            Console.WriteLine();
            Console.WriteLine(" ███████ ██   ██  █████  ██████  ██████   ██████ ██████  ");
            Console.WriteLine(" ██      ██   ██ ██   ██ ██   ██ ██   ██ ██           ██ ");
            Console.WriteLine(" ███████ ███████ ███████ ██████  ██████  ██       █████  ");
            Console.WriteLine("      ██ ██   ██ ██   ██ ██   ██ ██      ██      ██      ");
            Console.WriteLine(" ███████ ██   ██ ██   ██ ██   ██ ██       ██████ ███████ ");
            Console.WriteLine("                                                         ");
            Console.WriteLine("                            Adam   Chester (@_xpn_) ");
            Console.WriteLine("                            Daniel Duggan  (@_RastaMouse) ");
            Console.WriteLine();
        }

        private static void StartLogger()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(config)
                .CreateLogger();
        }

        private static string PromptForPass()
        {
            Console.Write(" [!] Enter server password: ");

            var pass = string.Empty;
            ConsoleKey key;

            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            } while (key != ConsoleKey.Enter);

            Console.WriteLine();
            Console.WriteLine();

            return pass;
        }
    }
}