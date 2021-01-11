using Clubby.Plugins;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Clubby.Discord.CommandHandling
{
    public class CommandHandler
    {
        private static Random rand = new Random();

        public PluginManager<IDiscordCommand,DiscordBot> commands = new PluginManager<IDiscordCommand,DiscordBot>();

        public Func<ulong, SocketTextChannel> GetChannel;

        [JsonIgnore]
        public Dictionary<ulong, IDiscordCommand> ExecutingCommand = new Dictionary<ulong, IDiscordCommand>();

        [JsonIgnore]
        private DiscordBot bot;

        public CommandHandler(DiscordBot bot)
        {
            this.bot = bot;
            if (Directory.Exists("./Commands"))
            {
                commands.Load("./Commands", bot);
            }
        }

        public void Reload()
        {
            commands.Load("./Commands",bot);
        }

        public Type[] GetAllCommands(int max_perms)
        {
            return commands.GetPlugins((f) =>
            {
                return (int)((IDiscordCommand)Activator.CreateInstance(f)).GetMinimumPerms() <= max_perms;
            }).ToArray();
        }

        public IDiscordCommand GetExecutingCommand(ulong id)
        {
            if (ExecutingCommand.ContainsKey(id))
                return ExecutingCommand[id];
            else return null;
        }

        public void SetExecutingCommand(ulong id, IDiscordCommand cmd)
        {
            if (ExecutingCommand.ContainsKey(id))
                ExecutingCommand[id] = cmd;
            else
                ExecutingCommand.Add(id, cmd);
        }


        public async Task Handle(SocketMessage msg, ulong current_user)
        {
            if (msg.Content.Trim().StartsWith($"<@{current_user}>") || msg.Content.Trim().StartsWith($"<@!{current_user}>"))
            {
                await msg.Channel.SendMessageAsync(null, false,
                    new EmbedBuilder()
                    .WithTitle("Clubby")
                    .AddField("The server prefix is:", $"'{Program.config.DiscordBotPrefix}'\nRun `{Program.config.DiscordBotPrefix}help` for more info")
                    .Build());
            }
            else if (ExecutingCommand.ContainsKey(msg.Author.Id) && ExecutingCommand[msg.Author.Id] != null)
            {
                if (msg.Content.StartsWith($"{Program.config.DiscordBotPrefix}cancel"))
                {
                    await msg.Channel.SendMessageAsync($"{ExecutingCommand[msg.Author.Id].GetCommandHelp().CommandName} command execution has been cancelled!");
                    ExecutingCommand[msg.Author.Id] = null;
                }
                else
                    await ExecutingCommand[msg.Author.Id].Handle(msg, (msg.Channel as SocketGuildChannel).Guild, this);
            }
            else if (msg.Content.StartsWith(Program.config.DiscordBotPrefix))
            {
                string command = ExtractCommand(msg.Content);
                IDiscordCommand cmd = commands.GetInstance(Construct_Identifier(command));
                if (cmd != null)
                {
                    int perms = (int)cmd.GetMinimumPerms();

                    int user_perms = Program.config.DiscordPermissions.GetResolvedPerms(msg.Author, msg.IsFromOwner());
                    if (user_perms >= perms)
                    {
                        if (rand.NextDouble() > 0.999)
                        {
                            await msg.Channel.SendMessageAsync("no");
                            return;
                        }
                        try
                        {
                            await cmd.Handle(msg, (msg.Channel as SocketGuildChannel).Guild, this);                            
                        }
                        catch(Exception e)
                        {
                            await msg.Channel.SendError(e.Message);
                        }
                    }
                    else
                        await msg.Channel.SendMessageAsync("You don't have permissions to use this command!");
                }
                else
                {
                    await msg.Channel.SendError($"Couldn't find command: `{Program.config.DiscordBotPrefix}{command}`");
                }
            }
        }

        private Func<Type, bool> Construct_Identifier(string cmd)
        {
            return (t) =>
            {
                return t.Name.ToLower() == cmd.ToLower();
            };
        }

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
