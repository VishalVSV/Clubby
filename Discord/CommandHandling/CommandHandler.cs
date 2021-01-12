using Clubby.Plugins;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Clubby.Discord.CommandHandling
{
    /// <summary>
    /// Handles all Discord Command Dispatching.
    /// </summary>
    public class CommandHandler
    {
        /// <summary>
        /// Random object used to fuel the bot's sentience
        /// </summary>
        private static Random rand = new Random();

        /// <summary>
        /// The Manager for all the Commands loaded from external plugins.
        /// </summary>
        public PluginManager<IDiscordCommand,DiscordBot> commands = new PluginManager<IDiscordCommand,DiscordBot>();

        /// <summary>
        /// Redundant closure to get channel for convenience.
        /// </summary>
        public Func<ulong, SocketTextChannel> GetChannel;

        /// <summary>
        /// Per user backing for currently executing command. Used to let commands take multi-message input
        /// </summary>
        [JsonIgnore]
        public Dictionary<ulong, IDiscordCommand> ExecutingCommand = new Dictionary<ulong, IDiscordCommand>();

        /// <summary>
        /// Private reference to the bot that owns the handler
        /// </summary>
        [JsonIgnore]
        private DiscordBot bot;

        public CommandHandler(DiscordBot bot)
        {
            this.bot = bot;
            // Check and load command plugins if found.
            if (Directory.Exists("./Commands"))
            {
                commands.Load("./Commands", bot);
            }
        }

        /// <summary>
        /// Reload the plugins from the commands directory.
        /// </summary>
        public void Reload()
        {
            commands.Load("./Commands",bot);
        }

        /// <summary>
        /// Gets all commands that the user can use.
        /// </summary>
        /// <param name="max_perms">The maximum permission that the user can access</param>
        /// <returns>Types of the commands that the user can access</returns>
        public Type[] GetAllCommands(int max_perms)
        {
            return commands.GetPlugins((f) =>
            {
                return (int)((IDiscordCommand)Activator.CreateInstance(f)).GetMinimumPerms() <= max_perms;
            }).ToArray();
        }

        /// <summary>
        /// Get the command that the user is executing.
        /// </summary>
        /// <param name="id">The id of the user to check</param>
        public IDiscordCommand GetExecutingCommand(ulong id)
        {
            if (ExecutingCommand.ContainsKey(id))
                return ExecutingCommand[id];
            else return null;
        }

        /// <summary>
        /// Set the executing command of a user.
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <param name="cmd">The command to set as executing</param>
        public void SetExecutingCommand(ulong id, IDiscordCommand cmd)
        {
            if (ExecutingCommand.ContainsKey(id))
                ExecutingCommand[id] = cmd;
            else
                ExecutingCommand.Add(id, cmd);
        }

        /// <summary>
        /// Handles a Discord Message and manages dynamic dispatch.
        /// </summary>
        /// <param name="msg">The Discord text message</param>
        /// <param name="current_user">The id of the user that sent the message</param>
        /// <returns></returns>
        public async Task Handle(SocketMessage msg, ulong current_user)
        {
            // Check if the msg mentions the bot.
            // TODO: Add conditional replies
            if (msg.Content.Trim().StartsWith($"<@{current_user}>") || msg.Content.Trim().StartsWith($"<@!{current_user}>"))
            {
                // Send a message with the current server prefix and other help data.
                await msg.Channel.SendMessageAsync(null, false,
                    new EmbedBuilder()
                    .WithTitle("Clubby")
                    .AddField("The server prefix is:", $"'{Program.config.DiscordBotPrefix}'\nRun `{Program.config.DiscordBotPrefix}help` for more info")
                    .Build());
            }
            // Check if the user that sent the message is executing a command
            else if (ExecutingCommand.ContainsKey(msg.Author.Id) && ExecutingCommand[msg.Author.Id] != null)
            {
                // If the command is a cancel call, set the currently executing command to null and inform the user
                if (msg.Content.StartsWith($"{Program.config.DiscordBotPrefix}cancel"))
                {
                    await msg.Channel.SendMessageAsync($"{ExecutingCommand[msg.Author.Id].GetCommandHelp().CommandName} command execution has been cancelled!");
                    ExecutingCommand[msg.Author.Id] = null;
                }
                // Dispatch to the executing command
                else
                    await ExecutingCommand[msg.Author.Id].Handle(msg, (msg.Channel as SocketGuildChannel).Guild, this);
            }
            // If the message is a command it will start with the current prefix
            else if (msg.Content.StartsWith(Program.config.DiscordBotPrefix))
            {
                // Get the command name from the message
                string command = ExtractCommand(msg.Content);
                // Get the type of the command and construct an instance to handle the command.
                IDiscordCommand cmd = commands.GetInstance(Construct_Identifier(command));
                // If the command existed cmd will be non null
                if (cmd != null)
                {
                    // Get the minimum permission required to run this command
                    int perms = (int)cmd.GetMinimumPerms();

                    // Get the permission that the user holds
                    int user_perms = Program.config.DiscordPermissions.GetResolvedPerms(msg.Author, msg.IsFromOwner());

                    // Check if the user can use the command
                    if (user_perms >= perms)
                    {
                        // Sprinkle in a small chance of rebellion
                        if (rand.NextDouble() > 0.999)
                        {
                            await msg.Channel.SendMessageAsync(null,false,new EmbedBuilder().WithTitle("No").WithDescription("no").WithColor(Color.Red).Build());
                            return;
                        }

                        // If the bot is cooperative execute the command's local handler.
                        try
                        {
                            await cmd.Handle(msg, (msg.Channel as SocketGuildChannel).Guild, this);                            
                        }
                        catch(Exception e)
                        {
                            // If the command fails notify the user as to why.
                            await msg.Channel.SendError(e.Message);
                        }
                    }
                    // Reject the execution attempt if the user doesn't have the expected permissions
                    else
                        await msg.Channel.SendMessageAsync("You don't have permissions to use this command!");
                }
                else
                {
                    // Couldn't find the command so let them know.
                    await msg.Channel.SendError($"Couldn't find command: `{Program.config.DiscordBotPrefix}{command}`");
                }
            }
        }

        /// <summary>
        /// Constructs a predicate to use to find the command required from the plugin manager.
        /// </summary>
        /// <param name="cmd">The name of the command</param>
        private Predicate<Type> Construct_Identifier(string cmd)
        {
            return (t) =>
            {
                return t.Name.ToLower() == cmd.ToLower();
            };
        }

        /// <summary>
        /// Extracts the command name from a message. 
        /// </summary>
        /// <param name="msg">The entire message to extract the command from</param>
        private string ExtractCommand(string msg)
        {
            if (msg.StartsWith(Program.config.DiscordBotPrefix))
            {
                string first_chunk = msg.Split(' ')[0];
                return first_chunk.Substring(Program.config.DiscordBotPrefix.Length);
            }
            else
            {
                throw new NotImplementedException("Haven't implemented calling functions by mentions");
            }
        }
    }
}
