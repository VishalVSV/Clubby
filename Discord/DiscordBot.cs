using Clubby.Discord.CommandHandling;
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
        DiscordSocketClient client;
        CommandHandler commandHandler;

        public async Task Start()
        {
            client = new DiscordSocketClient();
            commandHandler = new CommandHandler();

            commandHandler.GetChannel = (id) =>
            {
                return client.GetChannel(id) as SocketTextChannel;
            };

            client.Log += Log;
            client.MessageReceived += MessageReceived;
            client.Ready += Ready;

            await client.LoginAsync(TokenType.Bot, Program.config.DiscordBotToken);

            await client.StartAsync();
        }

        private Task Ready()
        {
            Program.config.GetChannel = (id) =>
            {
                return client.GetChannel(id) as SocketTextChannel;
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
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }
    }
}