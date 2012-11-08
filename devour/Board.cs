using SignalR.Hubs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace devour
{
    public class Board : Hub, IConnected
    {
        private static int[] board;
        
        static Board()
        {
            board = new int[2000];
        }

        public void Ping(int id)
        {
            var value = System.Threading.Interlocked.Increment(ref board[id]);
            Clients.Pong(id, value);
        }

        public void Reset()
        {
            board = new int[2000];
            Clients.Init(board);
            Clients.Toast("Board reset by " + Context.ConnectionId);
        }

        public Task Connect()
        {
            return Clients[Context.ConnectionId].Init(board);
        }

        public Task Reconnect(IEnumerable<string> groups)
        {
            return Clients[Context.ConnectionId].Init(board);
        }
    }
}