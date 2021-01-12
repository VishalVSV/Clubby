using Clubby.ConfigService;
using System;
using System.Threading.Tasks;
using Clubby.Discord;
using System.Threading;
using System.IO;
using Clubby.GeneralUtils;

namespace Clubby
{
    public class Program
    {
        public static ConfigFile config;
        public static bool stop = false;
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
                config.StartTime = DateTime.Now;
                if(File.Exists("./banner.ans"))
                {
                    Console.WriteLine(File.ReadAllText("./banner.ans"));
                }

                Console.ReadKey(true);

                config.DiscordBot = new DiscordBot();

                config.DiscordBot.Start().Wait();

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

                    if (config.DiscordBot.Disconnected && restart_signalled)
                    {
                        Logger.Log<Program>("Bot disconnected. If bot does not reconnect in 3 seconds it will be restarted!");
                        restart_signalled = true;
                        Task.Delay(3000).ContinueWith((_) =>
                        {
                            restart_signalled = false;
                            if (config.DiscordBot.Disconnected)
                            {
                                Logger.Log<Program>("Restarting bot");
                                config.DiscordBot = new DiscordBot();
                                config.DiscordBot.Start().Wait();
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

                config.DiscordBot.UpdateDashboard().Wait();
                config.DiscordBot.LogOut().Wait();
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
