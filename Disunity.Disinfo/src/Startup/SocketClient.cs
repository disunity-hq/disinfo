using Discord.WebSocket;


namespace Disunity.Disinfo.Startup {

    public class SocketClient : DiscordSocketClient {

        public SocketClient(SocketConfig config) : base(config) { }

    }

}