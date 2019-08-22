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

using CommandService = Disunity.Disinfo.Services.CommandService;


namespace Disunity.Disinfo {

    public class Startup {

        public IConfigurationRoot Configuration { get; }

        public Startup(IConfigurationRoot configuration) {
            Configuration = configuration;
        }

        private DiscordSocketClient ConstructDiscordSocketClient() {
            return new DiscordSocketClient(new DiscordSocketConfig {
                // Add discord to the collection
                LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000 // Cache 1,000 messages per channel
            });
        }

        private Discord.Commands.CommandService ConstructCommandService() {
            return new Discord.Commands.CommandService(new CommandServiceConfig {
                // Add the command service to the collection
                LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                DefaultRunMode = RunMode.Async, // Force all commands to run async by default
            });
        }

        public void ConfigureServices(IServiceCollection services) {
            services.AddSingleton(ConstructDiscordSocketClient())
                    .AddSingleton(ConstructCommandService())
                    .AddSingleton<CommandService>()
                    .AddSingleton<StartupService>()
                    .AddSingleton<LoggingService>()
                    .AddSingleton(Configuration)
                    .AddSingleton<DbService>()
                    .AddSingleton<FactService>()
                    .AddSingleton(new SlugHelper(
                                      new SlugHelper.Config() {
                                          CollapseDashes = true,
                                          CollapseWhiteSpace = true,
                                          ForceLowerCase = true
                                      }))
                    .AddTransient<LearnModule>();
        }

    }

}