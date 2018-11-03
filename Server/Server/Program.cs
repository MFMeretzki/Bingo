using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;


static class Program
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

    public static void Shuffle<T> (this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
