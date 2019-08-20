using System;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Microsoft.Extensions.Configuration;


namespace Disunity.Disinfo.Services {

    public class CommandService {

        private readonly DiscordSocketClient _discord;
        private readonly Discord.Commands.CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly LoggingService _logger;
        private readonly IServiceProvider _provider;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public CommandService(
            DiscordSocketClient discord,
            Discord.Commands.CommandService commands,
            IConfigurationRoot config,
            LoggingService logger,
            IServiceProvider provider) {
            _discord = discord;
            _commands = commands;
            _config = config;
            _logger = logger;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;
        }

        private void Info(string message) {
            _logger.OnLogAsync(new LogMessage(LogSeverity.Info, "CommandService", message));
        }

        private async Task OnMessageReceivedAsync(SocketMessage s) {
            var msg = s as SocketUserMessage; // Ensure the message is from a user/bot

            if (msg == null) {
                Info("Message was null.");
                return;
            }

            if (msg.Author.Id == _discord.CurrentUser.Id) {
                Info("Ignoring self.");
                return; // Ignore self when checking commands
            }

            if (msg.Author.IsBot) {
                Info("Ignoring bot.");
                return; // Ignore other bots
            }

            var guildUser = (SocketGuildUser) msg.Author;

            var canInteract = false;
            var rolesEnv = _config["Roles"] ?? "Administrator";
            Info($"Valid roles: {rolesEnv}");
            var validRoles = rolesEnv.Split(",").Select(o => o.ToLower());

            foreach (var role in guildUser.Roles.Select(r => r.Name)) {
                var lowercaseRole = role.ToLower();
                if (validRoles.Contains(lowercaseRole)) {
                    Info($"User role `{role}` is valid.");
                    canInteract = true;
                } else {
                    Info($"User role `{role}` is invalid.");
                }
            }

            if (!canInteract) {
                Info("Can't interact...");
                return;
            }

            var context = new SocketCommandContext(_discord, msg); // Create the command context

            int argPos = 0; // Check if the message has a valid command prefix

            if (msg.HasStringPrefix(_config["Prefix"], ref argPos) ||
                msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)) {
                var result = await _commands.ExecuteAsync(context, argPos, _provider); // Execute the command

                if (!result.IsSuccess) {
                    var args = msg.Content.Substring(argPos).Split();

                    if (args.Length == 1 && msg.Content.EndsWith("?")) {
                        var query = args[0].Substring(0, args[0].Length - 1);
                        await _commands.ExecuteAsync(context, $"what is {query}", _provider);
                    } else {
                        await context.Channel.SendMessageAsync(result.ToString());    
                    }
                    
                }
            }
        }

    }

}