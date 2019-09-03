using System;
using System.Linq;
using System.Threading.Tasks;

using Discord.Commands;
using Discord.WebSocket;


namespace Disunity.Disinfo.Attributes {

    [AttributeUsage(AttributeTargets.Method)]
    public class RolesAboveMeAttribute : PreconditionAttribute {

        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
                                                                       IServiceProvider services) {
            var ownName = context.Client.CurrentUser.Username;
            var ownRole = context.Guild.Roles.Single(r => r.Name == ownName);

            // Check if this user is a Guild User, which is the only context where roles exist
            if (!(context.User is SocketGuildUser gUser)) {
                return Task.FromResult(PreconditionResult.FromError("You must be in a guild to run this command."));
            }

            var result = gUser.Roles.Any(r => r.Position >= ownRole.Position)
                ? PreconditionResult.FromSuccess()
                : PreconditionResult.FromError($"You must have a role higher than {ownName} to run this command.");

            return Task.FromResult(result);

        }

    }

}