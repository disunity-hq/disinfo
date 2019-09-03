using BindingAttributes;

using Discord.Commands;


namespace Disunity.Disinfo.Services {

    [AsScoped]
    public class ContextService {

        public SocketCommandContext Context { get; set; }

    }

}