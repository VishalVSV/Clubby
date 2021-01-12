using Discord.WebSocket;
using System.Threading.Tasks;

namespace Clubby.Discord.CommandHandling
{
    /// <summary>
    /// The interface to define a Discord command. Used alongside the Command handler.
    /// </summary>
    public interface IDiscordCommand
    {
        /// <summary>
        /// The function that the command handling is deffered to.
        /// </summary>
        /// <param name="msg">The raw message that is being handled</param>
        /// <param name="guild">The guild that the message was from</param>
        /// <param name="commandHandler">The command handler that handled this command</param>
        Task Handle(SocketMessage msg,SocketGuild guild,CommandHandler commandHandler);

        /// <summary>
        /// Gets the minimum permissions required to exeucte the current command.
        /// </summary>
        DiscordCommandPermission GetMinimumPerms();

        /// <summary>
        /// Gets the command metadata.
        /// </summary>
        HelpDetails GetCommandHelp();
    }
}
