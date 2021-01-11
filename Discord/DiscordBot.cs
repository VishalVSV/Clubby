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
    public class DiscordBot
    {
        public DiscordSocketClient client;
        CommandHandler commandHandler;

        public bool Disconnected
        {
            get
            {
                return client.ConnectionState == ConnectionState.Disconnected;
            }
        }

        public async Task Start()
        {
            client = new DiscordSocketClient();


            client.Log += Log;
            client.MessageReceived += MessageReceived;
            client.Ready += Ready;


            await client.LoginAsync(TokenType.Bot, Program.config.DiscordBotToken);

            await client.StartAsync();
        }

        public async Task LogOut()
        {
            await client.LogoutAsync();
        }

        private Task Ready()
        {
            commandHandler = new CommandHandler(this);
            commandHandler.GetChannel = (id) =>
            {
                return client.GetChannel(id) as SocketTextChannel;
            };
            Program.config.GetChannel = (id) =>
            {
                return client.GetChannel(id) as SocketTextChannel;
            };
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
            if (arg.Author.Id == client.CurrentUser.Id)
                return;
            await commandHandler.Handle(arg,client.CurrentUser.Id);
        }

        private Task Log(LogMessage arg)
        {
            Logger.Log(this, $"[{arg.Severity}] {arg.Message}");
            return Task.CompletedTask;
        }
    }
}