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
using System.IO;
using Clubby.GeneralUtils;

namespace Clubby
{
    public class Program
    {
        public static ConfigFile config;
        static bool stop = false;
        static bool restart_signalled = false;

        static void Main(string[] args)
        {
            config = "./config.txt";

            Console.CancelKeyPress += Console_CancelKeyPress;

            if(config == null)
            {
                config = ConfigFile.MakeDefault("./config.txt");
                config.Save();

                Logger.Log<Program>("Couldn't find config file!");
                Logger.Log<Program>("Auto generated a blank config file...");
                Logger.Log<Program>("Exiting...");

                return;
            }

            try
            {
                if(File.Exists("./banner.ans"))
                {
                    Console.WriteLine(File.ReadAllText("./banner.ans"));
                }

                Console.ReadKey(true);

                DiscordBot bot = new DiscordBot();

                bot.Start().Wait();

                DateTime time_since_last_save = DateTime.Now;
                while (!stop)
                {
                    if (DateTime.Now - time_since_last_save > config.AutoSaveInterval)
                    {
                        time_since_last_save = DateTime.Now;
                        config.Save();
                    }
                    config.scheduler.Tick();
                    Thread.Sleep(10);

                    if (bot.Disconnected && restart_signalled)
                    {
                        Logger.Log<Program>("Bot disconnected. If bot does not reconnect in 3 seconds it will be restarted!");
                        restart_signalled = true;
                        Task.Delay(3000).ContinueWith((_) =>
                        {
                            restart_signalled = false;
                            if (bot.Disconnected)
                            {
                                Logger.Log<Program>("Restarting bot");
                                bot = new DiscordBot();
                                bot.Start().Wait();
                            }
                        });
                    }

                    while (Console.KeyAvailable)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            stop = true;
                            break;
                        }
                    }
                }

                bot.LogOut().Wait();
            }
            catch(Exception e)
            {
                Logger.Log<Program>(e.Message);
            }

            

            config.Destroy();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            stop = true;
        }
    }
}
