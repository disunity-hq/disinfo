using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BindingAttributes;

using Discord.Commands;
using Discord.WebSocket;

using Disunity.Disinfo.Attributes;
using Disunity.Disinfo.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace Disunity.Disinfo.Services {

    [AsSingleton]
    public class DispatchService {

        private readonly DiscordSocketClient _discord;
        private readonly LoggingService<CommandService> _logService;
        private readonly DispatchServiceOptions _options;
        private readonly ILogger<DispatchService> _logger;
        private readonly IServiceProvider _provider;
        private readonly IEnumerable<MethodInfo> _parsers;


        public DispatchService(
            LoggingService<CommandService> logService,
            IOptions<DispatchServiceOptions> options,
            ILogger<DispatchService> logger,
            IServiceProvider provider,
            DiscordSocketClient socketClient
            ) {
            _logService = logService;
            _options = options.Value;
            _logger = logger;
            _provider = provider;
            _discord = socketClient;

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

        private async Task<bool> CollectionHandler(IServiceProvider provider,
                                                   MethodInfo parser,
                                                   Match match) {

            var instance = (ModuleBase<SocketCommandContext>) provider.GetRequiredService(parser.DeclaringType);
            return await (Task<bool>) parser.Invoke(instance, new object[] {match});
        }

        private async Task<bool> ParameterHandler(IServiceProvider provider,
                                                  MethodInfo parser, 
                                                  Match match) {
            if (match.Groups.Count == 0) {
                return false;
            }

            var parameters = parser.GetParameters();

            if (match.Groups.Count - 1 != parameters.Length) {
                return false;
            }

            var instance = (ModuleBase<SocketCommandContext>) provider.GetRequiredService(parser.DeclaringType);

            object[] args;

            if (match.Groups.Count == 1) {
                args = new object[] {match.Groups[0].Value};
            } else {
                var objects = match.Groups.Skip(1)
                                   .Select(g => g.Value)
                                   .Select(o => o.Trim())
                                   .Cast<object>()
                                   .ToList();

                args = objects.ToArray();
            }

            return await (Task<bool>) parser.Invoke(instance, args);
        }

        private async Task<bool> ProcessParser(IServiceProvider provider, MethodInfo parser, string message) {
            var attr = parser.GetCustomAttribute<ParserAttribute>();
            var parameters = parser.GetParameters();
            var match = attr.Pattern.Match(message);

            if (!match.Success) {
                return false;
            }

            if (parameters.Length == 1 && parameters[0].ParameterType == typeof(Match)) {
                Console.WriteLine($"Handling message `{message}`");
                if (await CollectionHandler(provider, parser, match)) {
                    return true;
                }
            }

            return await ParameterHandler(provider, parser, match);
        }

        private async Task<bool> ProcessParsers(IServiceProvider provider, string message) {
            foreach (var parser in _parsers) {
                if (await ProcessParser(provider, parser, message)) {
                    return true;
                }
            }

            return false;
        }

        public async Task OnMessageReceivedAsync(SocketMessage s) {
            _logger.LogInformation("Message received...");

            var msg = s as SocketUserMessage; // Ensure the message is from a user/bot

            if (msg == null) {
                return;
            }

            if (ShouldIgnore(msg)) {
                return;
            }

            var argPos = 0; // Check if the message has a valid command prefix
            var context = new SocketCommandContext(_discord, msg); // Create the command context
 
            using (var scope = _provider.CreateScope()) {
                var provider = scope.ServiceProvider;

                // Load commands and modules into the command service
                var commandService = provider.GetRequiredService<CommandService>();
                commandService.Log += _logService.LogMessage;

                var contextService = provider.GetRequiredService<ContextService>();
                contextService.Context = context;
                
                
                await commandService.AddModulesAsync(Assembly.GetEntryAssembly(), provider);
                
                if (msg.HasStringPrefix(_options.Prefix, ref argPos) ||
                    msg.HasMentionPrefix(_discord.CurrentUser, ref argPos)) {
                    var message = msg.Content.Substring(argPos);

                    if (await ProcessParsers(provider, message)) {
                        return;
                    }

                    var result = await commandService.ExecuteAsync(context, argPos, _provider); // Execute the command

                    if (!result.IsSuccess) {
                        await context.Channel.SendMessageAsync(result.ToString());
                    }
                }
                
            }

        }

    }

}