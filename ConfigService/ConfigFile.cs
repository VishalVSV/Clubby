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
    public class ConfigFile
    {
        protected string filePath = "./config.txt";

        public string DiscordBotToken = "YOUR_BOT_TOKEN";
        public string DiscordBotPrefix = "+";
        public Dictionary<string, ulong> DiscordChannels = new Dictionary<string, ulong>();
        public DiscordPermissions DiscordPermissions = new DiscordPermissions();
        public List<(AnnouncementType, ulong)> DiscordAnnouncements = new List<(AnnouncementType, ulong)>();
        public Dictionary<ulong, Suggestion> DiscordSuggestions = new Dictionary<ulong, Suggestion>();
        public int DiscordSuggestionCount = 0;

        public List<Council> Councils = new List<Council>();

        [JsonIgnore]
        public Action<Action, string> Call;

        /// <summary>
        /// For all data that doesn't have a fixed place in the config.
        /// 
        /// NOTE: Only assign serializable objects to this field.
        /// </summary>
        public Dictionary<string, object> DynamicData = new Dictionary<string, object>();

        public Scheduler scheduler = new Scheduler();

        private FileSystemWatcher config_watcher;

        public const int CONFIG_RELOAD_ATTEMPTS = 5;

        public static JsonSerializerSettings settings = new JsonSerializerSettings();

        static ConfigFile()
        {
            settings.Error += delegate (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args)
            {
                Logger.Log<ConfigFile>(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };
        }

        [JsonIgnore]
        public Func<ulong, SocketTextChannel> GetChannel;

        public TimeSpan AutoSaveInterval = TimeSpan.FromMinutes(1);

        public void Save()
        {
            config_watcher.EnableRaisingEvents = false;
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented, settings));
            config_watcher.EnableRaisingEvents = true;
        }

        public void Destroy()
        {
            config_watcher.EnableRaisingEvents = false;
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, Formatting.Indented,settings));
        }

        public static ConfigFile MakeDefault(string filePath)
        {
            ConfigFile file = new ConfigFile();

            file.filePath = filePath;

            return file;
        }

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
        }
    }
}
