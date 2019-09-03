using System.ComponentModel.DataAnnotations;

using BindingAttributes;

using Discord;
using Discord.WebSocket;


namespace Disunity.Disinfo.Options {

    [Options("Discord")]
    public class ClientServiceOptions {

        [Required]
        public string Token { get; set; }

        public DiscordSocketConfig SocketConfig { get; }

        public ClientServiceOptions() {
            SocketConfig = new DiscordSocketConfig {
                LogLevel = LogSeverity.Verbose,
                MessageCacheSize = 1000
            };
        }


    }

}