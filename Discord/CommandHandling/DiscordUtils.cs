using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Clubby.Discord.CommandHandling
{
    public static class DiscordUtils
    {
        public static bool IsFromOwner(this SocketMessage msg)
        {
            var a = (msg.Channel as SocketGuildChannel);
            return a.Guild.OwnerId == msg.Author.Id;
        }

        public static async Task SendError(this ISocketMessageChannel channel,string error_msg)
        {
            await channel.SendMessageAsync(null, false, new EmbedBuilder().WithTitle("Error").WithDescription(error_msg).WithColor(Color.Red).Build());
        }

        public static async Task SendOk(this ISocketMessageChannel channel, string error_msg)
        {
            await channel.SendMessageAsync(null, false, new EmbedBuilder().WithTitle("Success").WithDescription(error_msg).WithColor(Color.Green).Build());
        }
    }
}
