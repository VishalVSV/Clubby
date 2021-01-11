using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Clubby.Discord.CommandHandling
{
    public interface IDiscordCommand
    {
        Task Handle(SocketMessage msg,SocketGuild guild,CommandHandler commandHandler);
        DiscordCommandPermission GetMinimumPerms();
        HelpDetails GetCommandHelp();
    }
}
