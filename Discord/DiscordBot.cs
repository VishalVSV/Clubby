using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clubby.Discord
{
    /// <summary>
    /// Implementation of the Discord bot to handle commands and more.
    /// </summary>
    public class DiscordBot
    {
        /// <summary>
        /// The client that handles communications between the bot and Discord.
        /// </summary>
        public DiscordSocketClient client;
        /// <summary>
        /// The command handler instance that defers handling to the plugins.
        /// </summary>
        CommandHandler commandHandler;

        /// <summary>
        /// Gets the connection state of the bot
        /// </summary>
        public bool Disconnected
        {
            get
            {
                return client.ConnectionState == ConnectionState.Disconnected;
            }
        }

        /// <summary>
        /// Start up the bot and listen to Discord.
        /// </summary>
        public async Task Start()
        {
            client = new DiscordSocketClient();


            client.Log += Log;
            client.MessageReceived += MessageReceived;
            client.Ready += Ready;


            await client.LoginAsync(TokenType.Bot, Program.config.DiscordBotToken);

            await client.StartAsync();
        }

        /// <summary>
        /// Logs the bot out of Discord.
        /// </summary>
        public async Task LogOut()
        {
            await client.LogoutAsync();
        }

        private async Task Ready()
        {
            commandHandler = new CommandHandler(this);

            // Setup the GetChannel closure
            commandHandler.GetChannel = (id) =>
            {
                return client.GetChannel(id) as SocketTextChannel;
            };

            // Setup the other GetChannel closure
            Program.config.GetChannel = (id) =>
            {
                return client.GetChannel(id) as SocketTextChannel;
            };

            // Setup the Call closure
            Program.config.Call = async (action, err) =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    var channel = client.GetChannel(Program.config.DiscordChannels["errors"]) as SocketTextChannel;
                    if (channel != null)
                    {
                        await channel.SendError(string.Format(err, e.Message));
                    }
                }
            };

            // Update the bot dashboard
            await UpdateDashboard();
        }

        /// <summary>
        /// Update the bot dashboard based on current state.
        /// </summary>
        /// <returns></returns>
        public async Task UpdateDashboard()
        {
            bool online = !Program.stop;
            if (Program.config.DiscordDashboard != null)
            {
                IMessage msg;
                if ((msg = GetMessage(Program.config.DiscordDashboard.Value)) != null)
                {
                    await (msg as RestUserMessage).ModifyAsync(m =>
                    {
                        m.Embed = GetDashboard(online);
                    });
                }
            }
        }

        /// <summary>
        /// Get a Discord Message object from the id type.
        /// </summary>
        /// <param name="msg">The discord message to get</param>
        /// <returns></returns>
        public static IMessage GetMessage(DiscordMessage msg)
        {
            SocketTextChannel channel;
            if ((channel = Program.config.GetChannel(msg.ChannelId) as SocketTextChannel) != null)
            {
                return channel.GetMessageAsync(msg.MessageId).Result;
            }
            return null;
        }

        /// <summary>
        /// Get an embed with all the dashboard details
        /// </summary>
        /// <param name="online">Is the bot online?</param>
        /// <returns></returns>
        public Embed GetDashboard(bool online)
        {
            StringBuilder plugins = new StringBuilder();
            int i = 1;
            commandHandler.commands.LoadedTypes.Keys.ToList().ForEach(s =>
            {
                plugins.AppendLine($"{i++}. {s}");
            });


            return new EmbedBuilder()
                .WithColor(online ? Color.Green : Color.Red)
                .WithTitle("Dashboard")
                .AddField("Loaded Plugins:", plugins.ToString())
                .AddField("Uptime:", Program.config.Uptime.ToPrettyString())
                .AddField("Number of suggestions so far:", Program.config.DiscordSuggestionCount)
                .AddField("Current prefix:", $"`{Program.config.DiscordBotPrefix}`")
                .Build();
        }

        private Task MessageReceived(SocketMessage arg)
        {
            // Do not process commands sent by the bot.
            if (arg.Author.Id == client.CurrentUser.Id)
                return Task.CompletedTask;

            // Defer handling to the command handler
            _ = commandHandler.Handle(arg, client.CurrentUser.Id);

            return Task.CompletedTask;
        }

        private Task Log(LogMessage arg)
        {
            Logger.Log(this, $"[{arg.Severity}] {arg.Message}");
            return Task.CompletedTask;
        }
    }
}