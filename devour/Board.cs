﻿using System;
using System.Globalization;
using Microsoft.AspNet.SignalR.Hubs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace devour
{
    public class Board : Hub
    {
        private static int[] Cells;
        public static readonly int BoardWidth = 100;
        public static readonly int BoardHeight = 56;
        public static readonly int PixelCount = BoardWidth * BoardHeight;

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