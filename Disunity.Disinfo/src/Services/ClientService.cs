using System;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.WebSocket;

using Disunity.Disinfo.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;


namespace Disunity.Disinfo.Services {

    [AsSingleton]
    public class ClientService {

        public ClientServiceOptions Options { get; }
        public DiscordSocketClient Client { get; }

        public ClientService(LoggingService<ClientService> clientLog,
                                    DiscordSocketClient client,
                                    DispatchService dispatch,
                                    IOptions<ClientServiceOptions> options) {

            Client = client;
            Client.Log += clientLog.LogMessage;
            Console.Error.WriteLine($"Attaching message delegate");
            Client.MessageReceived += dispatch.OnMessageReceivedAsync;
            
            Options = options.Value;

        }
        public async Task Start() {
            await Client.LoginAsync(TokenType.Bot, Options.Token);
            await Client.StartAsync();
        }

        [AsSingleton(typeof(DiscordSocketClient))]
        public static DiscordSocketClient DiscordSocketClientFactory(IServiceProvider sp) {
            var options = sp.GetRequiredService<IOptions<ClientServiceOptions>>();
            return new DiscordSocketClient(options.Value.SocketConfig);
        }

    }

}