using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.Commands;
using Discord.WebSocket;

using Disunity.Disinfo.Startup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Disunity.Disinfo.Services {

    [AsSingleton]
    public class DispatchService {

        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly ILogger<DispatchService> _logger;
        private readonly IServiceProvider _provider;
        private readonly IEnumerable<MethodInfo> _parsers;

        // DiscordSocketClient, CommandService, IConfigurationRoot, and IServiceProvider are injected automatically from the IServiceProvider
        public DispatchService(
            DiscordSocketClient discord,
            CommandService commands,
            IConfigurationRoot config,
            ILogger<DispatchService> logger,
            IServiceProvider provider) {
            _discord = discord;
            _commands = commands;
            _config = config;
            _logger = logger;
            _provider = provider;

            _discord.MessageReceived += OnMessageReceivedAsync;
            _parsers = FindAllParsers();
        }

        private IEnumerable<MethodInfo> FindAllParsers() {
            return Assembly.GetEntryAssembly()
                           .GetTypes()
                           .SelectMany(t => t.GetMethods())
                           .Where(m => m.GetCustomAttributes(typeof(ParserAttribute), false).Length > 0)
                           .Where(m => m.ReturnType == typeof(Task<bool>))
                           .ToArray();
        }

        private void Info(string message) {
            _logger.LogInformation(message);
        }

        private bool ShouldIgnore(SocketUserMessage message) {

            if (message.Author.Id == _discord.CurrentUser.Id) {
                return true; // Ignore self when checking commands
            }

            if (message.Author.IsBot) {
                return true; // Ignore other bots
            }

//            if (!UserCanInteract(message, (SocketGuildUser) message.Author)) {
//                return true;
//            }

            return false;
        }

//        private bool UserCanInteract(SocketUserMessage message, SocketGuildUser user) {
//            var rolesEnv = _config["Roles"] ?? "Administrator";
//            var validRoles = rolesEnv.Split(",").Select(o => o.ToLower()).ToList();
//
//            return user.Roles
//                       .Select(r => r.Name)
//                       .Select(role => role.ToLower())
//                       .Any(lowercaseRole => validRoles.Contains(lowercaseRole));
//        }

        private async Task<bool> CollectionHandler(SocketCommandContext context, 
                                                   MethodInfo parser,
                                                   ParserAttribute attr,
                                                   Match match) {

            var instance = (ModuleBase<SocketCommandContext>) _provider.GetRequiredService(parser.DeclaringType);
            return await (Task<bool>) parser.Invoke(instance, new object[] {context, match});
        }

        private async Task<bool> ParameterHandler(SocketCommandContext context, 
                                                  MethodInfo parser, 
                                                  ParserAttribute attr,
                                                  Match match) {
            if (match.Groups.Count == 0) {
                return false;
            }

            var parameters = parser.GetParameters();

            if (match.Groups.Count != parameters.Length) {
                return false;
            }

            var instance = (ModuleBase<SocketCommandContext>) _provider.GetRequiredService(parser.DeclaringType);

            object[] args;

            if (match.Groups.Count == 1) {
                args = new object[] {context, match.Groups[0].Value};
            } else {
                var objects = match.Groups.Skip(1)
                                   .Select(g => g.Value)
                                   .Select(o => o.Trim())
                                   .Cast<object>()
                                   .ToList();

                objects.Insert(0, context);
                args = objects.ToArray();
            }

            return await (Task<bool>) parser.Invoke(instance, args);
        }

        private async Task<bool> ProcessParser(SocketCommandContext context, MethodInfo parser, string message) {
            var attr = parser.GetCustomAttribute<ParserAttribute>();
            var parameters = parser.GetParameters();
            var match = attr.Pattern.Match(message);

            if (!match.Success) {
                return false;
            }

            if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Match)) {
                Console.WriteLine($"Handling message `{message}`");
                if (await CollectionHandler(context, parser, attr, match)) {
                    return true;
                }
            }

            return await ParameterHandler(context, parser, attr, match);
        }

        private async Task<bool> ProcessParsers(SocketCommandContext context, string message) {
            foreach (var parser in _parsers) {
                if (await ProcessParser(context, parser, message)) {
                    return true;
                }
            }

            return false;
        }

        private async Task OnMessageReceivedAsync(SocketMessage s) {
            var msg = s as SocketUserMessage; // Ensure the message is from a user/bot

            if (msg == null) {
                return;
            }

            if (ShouldIgnore(msg)) {
                return;
            }

            var argPos = 0; // Check if the message has a valid command prefix
            var context = new SocketCommandContext(_discord, msg); // Create the command context

            if (msg.HasStringPrefix(_config["Prefix"], ref argPos) ||
                msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)) {
                var message = msg.Content.Substring(argPos);

                if (await ProcessParsers(context, message)) {
                    return;
                }

                var result = await _commands.ExecuteAsync(context, argPos, _provider); // Execute the command

                if (!result.IsSuccess) {
                    await context.Channel.SendMessageAsync(result.ToString());
                }
            }
        }

    }

}