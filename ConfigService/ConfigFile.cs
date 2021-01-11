using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Clubby.Club;
using Clubby.Discord;
using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Clubby.Scheduling;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Clubby.ConfigService
{
    /// <summary>
    /// The class that contains all persistent data for the program
    /// </summary>
    public class ConfigFile
    {
        /// <summary>
        /// Path pointing to the file that this instance deals with.
        /// </summary>
        protected string filePath = "./config.txt";

        /// <summary>
        /// The bot token to access discord.
        /// </summary>
        public string DiscordBotToken = "YOUR_BOT_TOKEN";
        /// <summary>
        /// The prefix used to call commands over on discord land.
        /// </summary>
        public string DiscordBotPrefix = "+";
        /// <summary>
        /// Named channels used for various purposes inside the bot implementation and over plugins
        /// </summary>
        public Dictionary<string, ulong> DiscordChannels = new Dictionary<string, ulong>();
        /// <summary>
        /// The Permission handler for this instance of the bot.
        /// </summary>
        public DiscordPermissions DiscordPermissions = new DiscordPermissions();
        /// <summary>
        /// The list of all announcements made by the bot.
        /// </summary>
        public List<(AnnouncementType, ulong)> DiscordAnnouncements = new List<(AnnouncementType, ulong)>();
        /// <summary>
        /// A list of all suggestions made through the bot.
        /// </summary>
        public Dictionary<ulong, Suggestion> DiscordSuggestions = new Dictionary<ulong, Suggestion>();
        /// <summary>
        /// The number of suggestions made through the bot
        /// </summary>
        public int DiscordSuggestionCount = 0;

        
        public List<Council> Councils = new List<Council>();

        /// <summary>
        /// A closure that calls a function and relays errors to the discord frontend.
        /// </summary>
        [JsonIgnore]
        public Action<Action, string> Call;

        /// <summary>
        /// For all data that doesn't have a fixed place in the config.
        /// 
        /// NOTE: Only assign serializable objects to this field.
        /// </summary>
        public Dictionary<string, object> DynamicData = new Dictionary<string, object>();

        /// <summary>
        /// The scheduler for the program.
        /// 
        /// NOTE: Periodic functions must be cleared before plugin reload to prevent dangling references.
        /// </summary>
        public Scheduler scheduler = new Scheduler();

        /// <summary>
        /// The watcher that reloads the config on editing.
        /// </summary>
        private FileSystemWatcher config_watcher;

        /// <summary>
        /// The number of times the auto reloader tries to load the config before it gives up.
        /// </summary>
        public const int CONFIG_RELOAD_ATTEMPTS = 5;

        /// <summary>
        /// The json serializer settings to be used throught the program.
        /// </summary>
        public static JsonSerializerSettings settings = new JsonSerializerSettings();

        static ConfigFile()
        {
            settings.Error += delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                Logger.Log<ConfigFile>(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };
        }

        /// <summary>
        /// Closure that gets a channel from an id.
        /// </summary>
        [JsonIgnore]
        public Func<ulong, SocketTextChannel> GetChannel;

        /// <summary>
        /// The interval in which the config file saves itself.
        /// </summary>
        public TimeSpan AutoSaveInterval = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Save the config instance
        /// </summary>
        public void Save()
        {
            config_watcher.EnableRaisingEvents = false;
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented, settings));
            config_watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Saves the config file and disables the watcher.
        /// </summary>
        public void Destroy()
        {
            config_watcher.EnableRaisingEvents = false;
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented,settings));
        }

        /// <summary>
        /// Returns a default config instance.
        /// </summary>
        /// <param name="filePath">The file name to assign to the default instance</param>
        /// <returns>A config instance with default values</returns>
        public static ConfigFile MakeDefault(string filePath)
        {
            ConfigFile file = new ConfigFile();

            file.filePath = filePath;

            return file;
        }

        // Loads a config file from a path implicitly.
        // It is probably bad practise to overload a implicit operator for a constructor but I like it so its staying.
        public static implicit operator ConfigFile(string path)
        {
            ConfigFile config = MakeDefault(path);
            JsonConvert.PopulateObject(File.ReadAllText(path), config);
            if (config != null)
            {
                config.filePath = path;

                config.config_watcher = new FileSystemWatcher(Path.GetDirectoryName(path));
                config.config_watcher.Changed += config.Config_file_edited;
                config.config_watcher.EnableRaisingEvents = true;

                return config;
            }
            else
                return null;
        }

        /// <summary>
        /// Callback to reload the config on edit
        /// </summary>
        private void Config_file_edited(object sender, FileSystemEventArgs e)
        {
            for (int i = 0; i < CONFIG_RELOAD_ATTEMPTS; i++)
            {
                try
                {
                    if (File.Exists(e.FullPath) && Path.GetFullPath(e.FullPath) == Path.GetFullPath(filePath))
                    {
                        using (StreamReader sr = new StreamReader(e.FullPath))
                        {
                            JsonConvert.PopulateObject(sr.ReadToEnd(), this);
                            Logger.Log(this,"Config reloaded!");
                        }
                    }
                    return;
                }
                catch (IOException)
                {
                    Thread.Sleep(100);
                    continue;
                }
            }

            Logger.Log(this, "Config failed to reload! Save the file again if changes are important!");
        }
    }
}
