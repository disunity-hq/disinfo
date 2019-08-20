using System.Threading.Tasks;

using Discord.Commands;

using Microsoft.Extensions.Configuration;


namespace Disunity.Disinfo.Modules {

    public class EchoModule : ModuleBase<SocketCommandContext> {

        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly string _prefix;

        public EchoModule(CommandService service, IConfigurationRoot config) {
            _service = service;
            _config = config;
            _prefix = _config["Prefix"];
        }

        [Command("echo")]
        public async Task EchoAsync([Remainder] string command) {
            await ReplyAsync(command);
        }

    }

}