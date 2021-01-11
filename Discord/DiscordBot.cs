using Clubby.Discord.CommandHandling;
using Clubby.GeneralUtils;
using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
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

        private Task Ready()
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

            return Task.CompletedTask;
        }

        private async Task MessageReceived(SocketMessage arg)
        {
            // Do not process commands sent by the bot.
            if (arg.Author.Id == client.CurrentUser.Id)
                return;

            // Defer handling to the command handler
            await commandHandler.Handle(arg,client.CurrentUser.Id);
        }

        private Task Log(LogMessage arg)
        {
            Logger.Log(this, $"[{arg.Severity}] {arg.Message}");
            return Task.CompletedTask;
        }
    }
}