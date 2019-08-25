using BindingAttributes;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Slugify;


namespace Disunity.Disinfo.Startup {

    public class Startup {

        public IConfigurationRoot Configuration { get; }

        public Startup(IConfigurationRoot configuration) {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services) {
            BindingAttribute.ConfigureBindings(services, null);
            FactoryAttribute.ConfigureFactories(services, null);

            services
                .AddLogging(builder => builder.AddConsole())
                .AddSingleton(Configuration)
                .AddSingleton<ISlugHelper, SlugHelper>()
                .AddSingleton<CommandService>()
                .AddSingleton(new DiscordSocketConfig {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 1000,
                })
                .AddSingleton<DiscordSocketClient>();
        }

    }

}