using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class Program
{

    private static Server server = new Server();

    static void Main (string[] args)
    {
        Console.CancelKeyPress += new ConsoleCancelEventHandler(ExitCallback);

        Console.WriteLine("Starting the server...");
        if (args.Length > 0)
        {
            Console.WriteLine("Listening on IP address: " + args[0]);
            server.Start(args[0]);
        }
        else server.Start();
    }

    private static void ExitCallback (Object sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("Stopping the server...");
        server.Stop();
    }
}
