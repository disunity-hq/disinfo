using System;
using System.Reflection;
using System.Threading.Tasks;

using Discord;
using Discord.WebSocket;

using Disunity.Disinfo.Startup;

using Microsoft.Extensions.Configuration;


namespace Disunity.Disinfo.Services {

    public class StartupService {

        private readonly IServiceProvider _provider;
        private readonly SocketClient _discord;
        private readonly Discord.Commands.CommandService _commands;
        private readonly IConfigurationRoot _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public StartupService(
            IServiceProvider provider,
            SocketClient discord,
            Discord.Commands.CommandService commands,
            IConfigurationRoot config) {
            _provider = provider;
            _config = config;
            _discord = discord;
            _commands = commands;
        }

        public async Task StartAsync() {
            string discordToken = _config["DiscordToken"]; // Get the discord token from the config file

            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `DiscordToken` environment variable.");

            await _discord.LoginAsync(TokenType.Bot, discordToken); // Login to discord
            await _discord.StartAsync(); // Connect to the websocket

            // Load commands and modules into the command service
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _provider); 
        }

    }

}