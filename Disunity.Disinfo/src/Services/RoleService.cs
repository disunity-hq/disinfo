using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Disunity.Disinfo.Startup;


namespace Disunity.Disinfo.Services {

    public class RoleService {

        private readonly SocketClient _discord;

        public RoleService(SocketClient discord) {
            _discord = discord;
        }

        public string Username => _discord.CurrentUser.Username;

        public IRole OwnRole(ICommandContext context) {
            foreach (var role in context.Guild.Roles) {
                if (role.Name == Username) {
                    return role;
                }
            }

            return null;
        }

        public bool CanManage(ICommandContext context) {

            if (context.User.Id == context.Guild.OwnerId) {
                return true;
            }

            var ownRole = OwnRole(context);

            return ((IGuildUser) context.User)
                   .RoleIds
                   .Select(roleId => context.Guild.GetRole(roleId))
                   .Any(role => role.Position >= ownRole.Position);
        }

    }

}