using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Clubby.Discord.CommandHandling
{
    /// <summary>
    /// Utils to help with Discord.
    /// </summary>
    public static class DiscordUtils
    {
        /// <summary>
        /// Helper command to tell if message was from the owner of the server.
        /// </summary>
        /// <param name="msg">The message to check</param>
        public static bool IsFromOwner(this SocketMessage msg)
        {
            var a = (msg.Channel as SocketGuildChannel);
            return a != null && a.Guild.OwnerId == msg.Author.Id;
        }

        /// <summary>
        /// Send an error on a channel
        /// </summary>
        /// <param name="channel">The channel to send the error on</param>
        /// <param name="error_msg">The message to send as an error</param>
        public static async Task<RestUserMessage> SendError(this ISocketMessageChannel channel, string error_msg)
        {
            return await channel.SendMessageAsync(null, false, new EmbedBuilder().WithTitle("Error").WithDescription(error_msg).WithColor(Color.Red).Build());
        }

        /// <summary>
        /// Send an succeded message on a channel
        /// </summary>
        /// <param name="channel">The channel to send the success message on</param>
        /// <param name="success_msg">The message to send as success</param>
        public static async Task<RestUserMessage> SendOk(this ISocketMessageChannel channel, string success_msg)
        {
            return await channel.SendMessageAsync(null, false, new EmbedBuilder().WithTitle("Success").WithDescription(success_msg).WithColor(Color.Green).Build());
        }

        public static string[] Arguments(this SocketMessage msg)
        {
            List<string> args = new List<string>();

            string src = msg.Content;
            int i = 0;

            StringBuilder token = new StringBuilder();
            while (i < src.Length)
            {
                if (src[i] == '"')
                {
                    if (token.ToString() != "")
                    {
                        args.Add(token.ToString());
                        token.Clear();
                    }
                    i++;
                    while (i < src.Length && src[i] != '"')
                    {
                        token.Append(src[i++]);
                    }
                    i++;

                    if (token.ToString() != "")
                    {
                        args.Add(token.ToString());
                        token.Clear();
                    }
                }
                else if (src[i] == ' ')
                {
                    if (token.ToString() != "")
                    {
                        args.Add(token.ToString());
                        token.Clear();
                    }
                }
                else
                {
                    token.Append(src[i]);
                }
                i++;
            }

            if(token.ToString() != "")
            {
                args.Add(token.ToString());
            }

            return args.ToArray();
        }

        // Get a proper username all the time. Guaranteed to never return null.
        public static string ResolvedName(this SocketUser user)
        {
            if(user as IGuildUser != null)
            {
                if ((user as IGuildUser).Nickname != null)
                    return (user as IGuildUser).Nickname;
                else return user.Username;
            }
            else
            {
                return user.Username;
            }
        }
    }
}
