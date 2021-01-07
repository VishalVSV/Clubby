using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using Clubby.Discord;
using Clubby.Discord.CommandHandling;
using Clubby.Scheduling;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Clubby.ConfigService
{
    public class ConfigFile
    {
        protected string filePath = "./config.txt";

        public string DiscordBotToken;
        public string DiscordBotPrefix;
        public Dictionary<string, ulong> DiscordChannels = new Dictionary<string, ulong>();
        public DiscordPermissions DiscordPermissions;
        public List<(AnnouncementType,ulong)> DiscordAnnouncements = new List<(AnnouncementType,ulong)>();

        public Scheduler scheduler = new Scheduler();

        private FileSystemWatcher config_watcher;

        public const int CONFIG_RELOAD_ATTEMPTS = 5;

        public static JsonSerializerSettings settings = new JsonSerializerSettings();

        [JsonIgnore]
        public Func<ulong, SocketTextChannel> GetChannel;

        public TimeSpan AutoSaveInterval = TimeSpan.FromMinutes(1);

        static ConfigFile()
        {

        }

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

        public static implicit operator ConfigFile(string path)
        {
            ConfigFile config = JsonConvert.DeserializeObject<ConfigFile>(File.ReadAllText(path),settings);
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
                            Console.WriteLine("Config reloaded!");
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
