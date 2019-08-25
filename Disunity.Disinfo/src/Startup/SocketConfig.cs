using Discord;
using Discord.WebSocket;


namespace Disunity.Disinfo.Startup {

    public class SocketConfig : DiscordSocketConfig {

        public SocketConfig() {
            LogLevel = LogSeverity.Verbose;
            MessageCacheSize = 1000;
        }        

    }

}