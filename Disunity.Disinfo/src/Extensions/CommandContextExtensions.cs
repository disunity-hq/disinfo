using System.Linq;

using Discord;
using Discord.Commands;


namespace Disunity.Disinfo.Extensions {

    public static class CommandContextExtensions {

        private static int RolePosition(ICommandContext context, ulong id) {
            return context.Guild.GetRole(id).Position;
        }

        private static bool IsAdmin(ICommandContext context, IGuildUser user, IRole role) {
            return user.RoleIds.Any(id => RolePosition(context, id) >= RolePosition(context, role.Id));
        }

        public static bool IsAdmin(this ICommandContext context) {
            var selfName = context.Client.CurrentUser.Username;
            var selfRole = context.Guild.Roles.Single(r => r.Name == selfName);
            var guildUser = (IGuildUser) context.User;
            return IsAdmin(context, guildUser, selfRole);
        }

    }

}