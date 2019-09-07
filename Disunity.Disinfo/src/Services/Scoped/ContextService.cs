using System.Linq;

using BindingAttributes;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Data;


namespace Disunity.Disinfo.Services {

    [AsScoped]
    public class ContextService {

        public SocketCommandContext Context { get; set; }
        
        public IApplication Application { get; set; }

        public bool IsOwner => Application.Owner.Id == Context.User.Id;

        public bool IsDM => Context.Channel is IDMChannel;

        public bool IsManagement => IsOwner && IsDM;

        public string Guild => IsManagement ? "0" : Context.Guild.Id.ToString();
        
        public DisinfoDbContext Db { get; set; }

        
        private int RolePosition(ulong id) {
            return Context.Guild.GetRole(id).Position;
        }

        private bool HasAdmin(IGuildUser user, IRole role) {
            return user.RoleIds.Any(id => RolePosition(id) >= RolePosition(role.Id));
        }

        public bool IsAdmin {
            get {

                if (IsOwner) return true;

                if (IsDM) return false;
                
                var selfName = Context.Client.CurrentUser.Username;

                if (Context.Guild.Roles.Count == 0) {
                    return true;
                }

                var myRoles = Context.Guild.Roles.Where(r => r.Name == selfName);

                if (!myRoles.Any()) {
                    return true;
                }

                var selfRole = myRoles.First();
                var guildUser = (IGuildUser) Context.User;
                return HasAdmin(guildUser, selfRole);
            }
        }

    }

}