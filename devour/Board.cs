using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace devour
{
    public class Board : Hub
    {
        private static int[] Cells;
        public static readonly int BoardWidth = 100;
        public static readonly int BoardHeight = 56;
        public static readonly int PixelCount = BoardWidth * BoardHeight;
        public static DateTime LastSaved = DateTime.UtcNow;

        static Board()
        {
            Cells = new int[PixelCount];
        }

        public int Ping(int id)
        {
            var value = Interlocked.Increment(ref Cells[id]);
            Clients.All.Pong(id, value);

            return value;
        }

        public async void DumpBoard(int[] board)
        {
            // make sure table exists:

//var checkTableCommandText = "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo' AND  TABLE_NAME = 'BoardHistory') BEGIN CREATE TABLE BoardHistory(Id INT IDENTITY PRIMARY KEY, Values varbinary(max) END
            const string commandText = "INSERT INTO BoardHistory (Values) VALUES (@Values)";
            using (var connection = new SqlConnection())
            {
                var command = new SqlCommand(commandText, connection);
                var bytes = ToBytes(board);

                command.Parameters.Add("@Values", SqlDbType.Binary, bytes.Length).Value = bytes;

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        // via http://stackoverflow.com/a/713105/29
        private byte[] ToBytes(int[] ints)
        {
            // basic - same count
            bool[] bools = Array.ConvertAll(ints, b => b%2 == 1);
            byte[] arr1 = Array.ConvertAll(bools, b => b ? (byte)1 : (byte)0);

            // pack (in this case, using the first bool as the lsb - if you want
            // the first bool as the msb, reverse things ;-p)
            int bytes = bools.Length / 8;
            if ((bools.Length % 8) != 0) bytes++;
            byte[] arr2 = new byte[bytes];
            int bitIndex = 0, byteIndex = 0;
            for (int i = 0; i < bools.Length; i++)
            {
                if (bools[i])
                {
                    arr2[byteIndex] |= (byte)(((byte)1) << bitIndex);
                }
                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return arr2;
        }

        public void Reset()
        {
            Cells = new int[PixelCount];
            Clients.All.Init();
            Clients.Caller.Toast("Board reset...");
            Clients.Others.Toast("Board reset by another user...");
        }

        public override Task OnConnected()
        {
            Clients.Caller.Toast("Welcome! Just mouse around...");
            Clients.Others.Toast("Another user has joined!");
            Clients.Caller.Init(Cells);
            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            Clients.Caller.Init(Cells);
            return base.OnReconnected();
        }
    }
}