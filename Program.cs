using Clubby.ConfigService;
using System;
using System.Threading.Tasks;
using Clubby.Discord;
using System.Threading;
using System.IO;
using Clubby.GeneralUtils;
using System.Linq;
using System.Reflection;

namespace Clubby
{
    public class Program
    {
        /// <summary>
        /// The master config file.
        /// </summary>
        public static ConfigFile config;
        /// <summary>
        /// Gets whether the program should stop.
        /// </summary>
        public static bool stop = false;
        /// <summary>
        /// Has a restart already been signalled?
        /// </summary>
        static bool restart_signalled = false;

        static void Main(string[] args)
        {
            // Get config file
            config = "./config.json";

            // Reload string to force the config file to create a sheets handler.
            // I know its a hacky solution but hey it works...
            config.RegisterFileId = config.RegisterFileId;

            // Disable ctrl+C killing the bot. We can do that part ourselves.
            Console.CancelKeyPress += Console_CancelKeyPress;

            // If there wasn't a config file generate a new one.
            if (config == null)
            {
                config = ConfigFile.MakeDefault("./config.json");
                config.Save();

                Logger.Log<Program>("Couldn't find config file!");
                Logger.Log<Program>("Auto generated a blank config file...");
                Logger.Log<Program>("Exiting...");

                return;
            }

            try
            {
                // Set start time
                config.StartTime = DateTime.Now;

                // Show swag banner if it exists
                if (File.Exists("./banner.ans"))
                {
                    Console.WriteLine(File.ReadAllText("./banner.ans"));
                }

                if (args.Contains("--pause"))
                    Console.ReadKey(true);

                // Initialize the bot
                config.DiscordBot = new DiscordBot();

                config.DiscordBot.Start().Wait();

                // Start autosaving logic
                DateTime time_since_last_save = DateTime.Now;
                while (!stop)
                {
                    if (DateTime.Now - time_since_last_save > config.AutoSaveInterval)
                    {
                        time_since_last_save = DateTime.Now;
                        config.Save();
                    }
                    // Tick the scheduler
                    config.scheduler.Tick();

                    // Sleep to not kill the CPU
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

                    // Non blocking key per key reading.
                    while (Console.KeyAvailable)
                    {
                        if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                        {
                            Logger.Log<Program>("Stopping");
                            stop = true;
                            break;
                        }
                    }
                }

                config.DiscordBot.UpdateDashboard().Wait();
                config.DiscordBot.LogOut().Wait();
            }
            catch (Exception e)
            {
                Logger.Log<Program>(e.Message);
            }

            config.Destroy();
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Logger.Log<Program>("Stopping");
            e.Cancel = true;
            stop = true;
        }
    }
}
