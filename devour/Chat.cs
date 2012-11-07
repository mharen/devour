using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;

namespace devour
{
    public class Chat : Hub
    {
        private static int[,] board;
        
        static Chat()
        {
            board = new int[8,8];
        }

        public void Send(string message)
        {
            // Call the addMessage method on all clients
            Clients.addMessage(message);
        }
    }
}