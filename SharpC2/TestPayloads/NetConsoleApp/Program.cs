using System;

namespace NetConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Console.WriteLine($"You said {args[0]}");
            }
            else
            {
                Console.WriteLine($"You said nothing");
            }
        }
    }
}