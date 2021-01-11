[PluginExport]
public class Example : IDiscordCommand
{
    public HelpDetails GetCommandHelp()
    {
		throw new NotImplementedException();
        return Utils.Init((ref HelpDetails h) =>
        {
            h.CommandName = "example";
            h.ShortDescription = "Example command";
        });
    }

    public DiscordCommandPermission GetMinimumPerms()
    {
		throw new NotImplementedException();
        return DiscordCommandPermission.Member;
    }

    public async Task Handle(SocketMessage msg, SocketGuild guild, CommandHandler commandHandler)
    {
		throw new NotImplementedException();
    }
}