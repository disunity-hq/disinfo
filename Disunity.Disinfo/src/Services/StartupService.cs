using System;
using System.Reflection;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.WebSocket;

using Disunity.Disinfo.Startup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;


namespace Disunity.Disinfo.Services {

    [AsSingleton]
    public class StartupService {

        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _discord;
        private readonly Discord.Commands.CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly ILogger<StartupService> _log;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            DiscordSocketClient discord,
            Discord.Commands.CommandService commands,
            IConfigurationRoot config,
            ILogger<StartupService> log) {
            _provider = provider;
            _config = config;
            _log = log;
            _discord = discord;
            _commands = commands;
        }

        public Task LogMessage(LogMessage message) {
            LogLevel severity;

            switch (message.Severity) {
                case LogSeverity.Critical: {
                    severity = LogLevel.Critical;
                    break;
                }

                case LogSeverity.Debug: {
                    severity = LogLevel.Debug;
                    break;
                }

                case LogSeverity.Info: {
                    severity = LogLevel.Information;
                    break;
                }

                case LogSeverity.Warning: {
                    severity = LogLevel.Warning;
                    break;
                }

                case LogSeverity.Verbose: {
                    severity = LogLevel.Trace;
                    break;
                }

                default: {
                    severity = LogLevel.Information;
                    break;
                }
            }
                
            _log.Log(severity, message.Message);
            
            return Task.CompletedTask;
        }

        public async Task StartAsync() {
            string discordToken = _config["DiscordToken"]; // Get the discord token from the config file

            _discord.Log += LogMessage;
            _commands.Log += LogMessage;
        
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `DiscordToken` environment variable.");

            await _discord.LoginAsync(TokenType.Bot, discordToken); // Login to discord
            await _discord.StartAsync(); // Connect to the websocket

            // Load commands and modules into the command service
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider); 
        }

    }

}