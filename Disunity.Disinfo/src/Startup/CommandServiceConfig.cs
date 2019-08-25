using BindingAttributes;

using Discord;
using Discord.Commands;


namespace Disunity.Disinfo.Startup {

    [AsSingleton]
    public class CommandServiceConfig : Discord.Commands.CommandServiceConfig {

        public CommandServiceConfig() {
            LogLevel = LogSeverity.Verbose;
            DefaultRunMode = RunMode.Async;
        }

    }

}