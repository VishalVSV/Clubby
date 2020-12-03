using Clubby.ConfigService;
using System;
using Clubby.Events;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace Clubby
{
    class Program
    {
        static void Main(string[] args)
        {
            Config config = new Config();
            BetterEventHandler.SetConfig(config);
        }
    }
}
