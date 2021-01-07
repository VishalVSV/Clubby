using Clubby.ConfigService;
using System;
using Clubby.Events;
using System.Collections.Generic;
using Discord.WebSocket;
using System.Threading.Tasks;
using Clubby.Discord.CommandHandling;
using Clubby.Discord;
using System.Globalization;
using System.Threading;

namespace Clubby
{
    public class Program
    {
        public static ConfigFile config;

        static void Main(string[] args)
        {
            string format = "Test {0:d}";
            string a = string.Format(format, DateTime.Now);

            config = "./config.txt";

            DiscordBot bot = new DiscordBot();

            bot.Start().Wait();

            bool stop = false;
            DateTime time_since_last_save = DateTime.Now;
            while (!stop)
            {
                if(DateTime.Now - time_since_last_save > config.AutoSaveInterval)
                {
                    time_since_last_save = DateTime.Now;
                    config.Save();
                }
                config.scheduler.Tick();
                Thread.Sleep(10);
                while (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        stop = true;
                        break;
                    }
                }
            }

            config.Destroy();
        }
    }
}
