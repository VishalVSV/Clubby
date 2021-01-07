using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace Clubby.Discord.CommandHandling
{
    public enum DiscordCommandPermission : int
    {
        Member = 0,
        Moderator,
        Admin,
        Owner,
    }

    public class DiscordPermissions
    {
        public Dictionary<ulong, DiscordCommandPermission> User_Permissions = new Dictionary<ulong, DiscordCommandPermission>();
        public Dictionary<ulong, DiscordCommandPermission> Role_Permissions = new Dictionary<ulong, DiscordCommandPermission>();

        public int GetResolvedPerms(SocketUser user,bool owner = false)
        {
            if (owner)
                return (int)DiscordCommandPermission.Owner;
            int max_perm = int.MinValue;
            foreach (SocketRole role in (user as SocketGuildUser).Roles)
            {
                max_perm = Math.Max(max_perm, Program.config.DiscordPermissions.GetPerms(role, owner));
            }


            int user_perms = Math.Max(max_perm, Program.config.DiscordPermissions.GetPerms(user, owner));

            return user_perms;
        }

        public int GetPerms(SocketUser user,bool owner = false)
        {
            if (User_Permissions.ContainsKey(user.Id))
            {
                return (int)User_Permissions[user.Id];
            }
            else
            {
                if (owner)
                    return (int)DiscordCommandPermission.Owner;
                else
                {
                    User_Permissions.Add(user.Id, DiscordCommandPermission.Member);
                    return (int)User_Permissions[user.Id];
                }
            }
        }

        public int GetPerms(SocketRole role,bool owner = false)
        {
            if (Role_Permissions.ContainsKey(role.Id))
            {
                return (int)Role_Permissions[role.Id];
            }
            else
            {
                if (owner)
                    return (int)DiscordCommandPermission.Owner;
                else
                {
                    Role_Permissions.Add(role.Id, DiscordCommandPermission.Member);
                    return (int)Role_Permissions[role.Id];
                }
            }
        }

        public void SetUserPerms(ulong id,DiscordCommandPermission permission)
        {
            if (User_Permissions.ContainsKey(id))
                User_Permissions[id] = permission;
            else
                User_Permissions.Add(id, permission);
        }
    }
}
