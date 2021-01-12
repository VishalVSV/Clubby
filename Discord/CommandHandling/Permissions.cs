using Discord.WebSocket;
using System;
using System.Collections.Generic;

namespace Clubby.Discord.CommandHandling
{
    /// <summary>
    /// The enum of all possible discord permissions.
    /// </summary>
    public enum DiscordCommandPermission : int
    {
        /// <summary>
        /// The lowest permission. Given by default to everyone.
        /// </summary>
        Member = 0,
        /// <summary>
        /// A buffer permission. Currently unused.
        /// </summary>
        Moderator,
        /// <summary>
        /// The role with most permissions. Allows users to use most commands
        /// </summary>
        Admin,
        /// <summary>
        /// The highest role. Allows users to use all the commands.
        /// </summary>
        Owner,
    }

    /// <summary>
    /// Handles the permissions for all users managed by the bot.
    /// </summary>
    public class DiscordPermissions
    {
        /// <summary>
        /// Backing for permissions of users.
        /// </summary>
        public Dictionary<ulong, DiscordCommandPermission> User_Permissions = new Dictionary<ulong, DiscordCommandPermission>();
        /// <summary>
        /// Backing for permissions of roles.
        /// </summary>
        public Dictionary<ulong, DiscordCommandPermission> Role_Permissions = new Dictionary<ulong, DiscordCommandPermission>();

        /// <summary>
        /// Gets the permission of a user. Resolves both roles and user and returns the highest one.
        /// </summary>
        /// <param name="user">The user to check permissions of</param>
        /// <param name="owner">Is the user the owner?</param>
        public int GetResolvedPerms(SocketUser user,bool owner = false)
        {
            // If the user is an owner all the following logic is redundant so return immediately
            if (owner)
                return (int)DiscordCommandPermission.Owner;

            // Set max_perm to the smallest value to find the maximum permission among the roles assigned to the user.
            int max_perm = int.MinValue;
            foreach (SocketRole role in (user as SocketGuildUser).Roles)
            {
                max_perm = Math.Max(max_perm, Program.config.DiscordPermissions.GetRolePerms(role, owner));
            }

            // Get the highest of user and role permissions
            int user_perms = Math.Max(max_perm, Program.config.DiscordPermissions.GetUserPerms(user, owner));

            return user_perms;
        }

        /// <summary>
        /// Get user permissions without resolution
        /// </summary>
        /// <param name="user">The user to check permissions of</param>
        /// <param name="owner">Is the user the owner?</param>
        public int GetUserPerms(SocketUser user,bool owner = false)
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

        /// <summary>
        /// Get role permissions without resolution
        /// </summary>
        /// <param name="user">The role to check permissions of</param>
        /// <param name="owner">Is the user the owner?</param>
        public int GetRolePerms(SocketRole role,bool owner = false)
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

        /// <summary>
        /// Set the permissions of a user.
        /// </summary>
        /// <param name="id">The id of the user</param>
        /// <param name="permission">The permission to assign</param>
        public void SetUserPerms(ulong id,DiscordCommandPermission permission)
        {
            if (User_Permissions.ContainsKey(id))
                User_Permissions[id] = permission;
            else
                User_Permissions.Add(id, permission);
        }

        /// <summary>
        /// Set the permissions of a role.
        /// </summary>
        /// <param name="id">The id of the role</param>
        /// <param name="permission">The permission to assign</param>
        public void SetRolePerms(ulong id, DiscordCommandPermission permission)
        {
            if (Role_Permissions.ContainsKey(id))
                Role_Permissions[id] = permission;
            else
                Role_Permissions.Add(id, permission);
        }
    }
}
