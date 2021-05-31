﻿// https://docs.stillu.cc/guides/commands/preconditions.html

using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBot.Attributes
{
    // Inherit from PreconditionAttribute
    public class RequireRoleAttribute : PreconditionAttribute
    {
        // Create a field to store the specified name
        private readonly string _name;

        // Create a constructor so the name can be specified
        public RequireRoleAttribute(string name) => _name = name;

        // Override the CheckPermissions method
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            // Check if this user is a Guild User, which is the only context where roles exist
            if (context.User is SocketGuildUser gUser)
            {
                var user = await context.Guild.GetUserAsync(context.User.Id) as SocketGuildUser;
                // If this command was executed by a user with the appropriate role, return a success
                if (gUser.Roles.Any(r => r.Name.ToLowerInvariant() == _name.ToLowerInvariant()))
                    // Since no async work is done, the result has to be wrapped with `Task.FromResult` to avoid compiler errors
                    return await Task.FromResult(PreconditionResult.FromSuccess());
                // Since it wasn't, fail
                else
                    return await Task.FromResult(PreconditionResult.FromError($"You must have a role named {_name} to run this command."));
            }
            else
                return await Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
        }
    }
}
