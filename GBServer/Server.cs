using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace GBServer
{
    internal class Server : WebSocketBehavior
    {
        private static List<string> ids = new List<string>();

        protected override void OnOpen()
        {
            Console.WriteLine($"User {ID} connected...");
            ids.Add(ID);
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            foreach(string id in ids)
            {
                if (id != ID) Sessions.SendTo(id, e.RawData);
            }
        }

        protected override void OnError(ErrorEventArgs e)
        {
            Console.WriteLine($"Error for {ID}: {e.Message}");
        }

        protected override void OnClose(CloseEventArgs e)
        {
            Console.WriteLine($"User {ID} disconnected...");
            ids.Remove(ID);
        }
    }
}
