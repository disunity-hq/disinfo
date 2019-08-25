using System;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Disunity.Disinfo.Modules;
using Disunity.Disinfo.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Slugify;


namespace Disunity.Disinfo.Startup {

    public class Startup {

        public IConfigurationRoot Configuration { get; }

        public Startup(IConfigurationRoot configuration) {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services) {
            services
                .AddSingleton(Configuration)
                .AddSingleton<SlugHelperConfig>()
                .AddSingleton<ISlugHelper, SlugHelper>()
                .AddSingleton<CommandServiceConfig>()
                .AddSingleton<CommandService>()
                .AddSingleton<SocketConfig>()
                .AddSingleton<SocketClient>()
                .AddSingleton<LoggingService>()
                .AddSingleton<StartupService>()
                .AddSingleton<DispatchService>()
                .AddSingleton<DbService>()
                .AddSingleton<FactService>()
                .AddSingleton<RoleService>()
                .AddTransient<LearnModule>();
        }

    }

}