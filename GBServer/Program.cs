using System;
using WebSocketSharp.Server;

namespace GBServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");
            WebSocketServer server = new WebSocketServer(8001);
            server.AddWebSocketService<Server>("/GBSharp");
            server.Start();
            Console.WriteLine("Server started!");
            Console.WriteLine("Press any key to end");
            Console.ReadKey();
            server.Stop();
        }
    }
}
