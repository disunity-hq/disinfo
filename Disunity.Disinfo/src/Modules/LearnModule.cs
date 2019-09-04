using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Attributes;
using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Services;
using Disunity.Disinfo.Services.Scoped;
using Disunity.Disinfo.Services.Singleton;

using Newtonsoft.Json;

using SharpYaml;


namespace Disunity.Disinfo.Modules {

    [AsScoped]
    public class LearnModule : ModuleBase<SocketCommandContext> {

        private readonly ContextService _contextService;
        private readonly EmbedService _embeds;
        private readonly RoleService _roles;
        private readonly LearnModuleService _learnService;
        
        public LearnModule(ContextService contextService, 
                           EmbedService embeds, 
                           RoleService roles, 
                           LearnModuleService learnService) {
            _contextService = contextService;
            _embeds = embeds;
            _roles = roles;
            _learnService = learnService;
        }
        
        public Task ReplyAsync(string message = null, Embed embed = null) {
            return _contextService.Context.Channel.SendMessageAsync(message, embed: embed);
        }

        public async Task<bool> CheckAccess(string input) {
            if (_contextService.IsManagement) return true;
            if (_contextService.IsDM && !_contextService.IsOwner) return false;

            var reply = new EmbedBuilder();
            var entry = _embeds.Query(input, _contextService.Guild);

            if (entry == null) return true;

            if (entry.Guild == "0" && !_contextService.IsOwner) {
                reply.WithTitle("Access denied")
                     .WithDescription("Only the owner can override global entries.");

                await ReplyAsync(embed: reply.Build());
                return false;
            }

            if (entry.Locked && !_contextService.IsAdmin) {
                reply.WithTitle("Access denied")
                     .WithDescription("Only the admins can override locked entries.");

                await ReplyAsync(embed: reply.Build());
                return false;
            }

            return true;
        }
        public async Task<Embed> LearnJson(string input, string json) {
            try {
                return _learnService.LearnJson(input, json);
            }
            catch (JsonException e) {

                var fields = e.Data.Keys
                              .Cast<string>()
                              .Select(k => new EmbedFieldBuilder().WithName(k).WithValue(e.Data[k]))
                              .ToList();

                fields.Insert(0, new EmbedFieldBuilder().WithName("Message").WithValue(e.Message));

                var reply = new EmbedBuilder()
                            .WithDescription("Couldn't parse that as JSON.")
                            .WithTitle("JSON Parse Failure")
                            .WithFields(fields)
                            .Build();

                await ReplyAsync(embed: reply);
                return null;
            }
            
        }

        public async Task<Embed> LearnYaml(string input, string yaml) {
            Dictionary<string, string> factData;

            try {
                return _learnService.LearnYaml(input, yaml);
            }
            catch (YamlException e) {
                var fields = e.Data.Keys
                              .Cast<string>()
                              .Select(k => new EmbedFieldBuilder().WithName(k).WithValue(e.Data[k]))
                              .ToList();

                fields.Insert(0, new EmbedFieldBuilder().WithName("Message").WithValue(e.Message.Split("): ", 2)[1]));
                fields.Add(new EmbedFieldBuilder().WithName("Start").WithValue(e.Start).WithIsInline(true));
                fields.Add(new EmbedFieldBuilder().WithName("End").WithValue(e.End).WithIsInline(true));

                var reply = new EmbedBuilder()
                            .WithDescription("Couldn't parse that as YAML.")
                            .WithTitle("YAML Parse Failure")
                            .WithFields(fields)
                            .Build();

                await ReplyAsync(embed: reply);
                return null;
            }
        }



        // forget <fact|prop>, <fact|prop>, ...
        [Parser(@"^(?i)forget (?:(?:,\s*)*([^,]+))*")]
        public async Task<bool> ParserForget(Match match) {
            var reply = _learnService.ForgetMatches(match).Build();
            await ReplyAsync(embed: reply);
            return true;
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?```(.*?)\n(.*)```")]
        public async Task<Embed> ParserLearnEmbed(string input, string format, string data) {
            if (!await CheckAccess(input)) return null;
            
            format = format.ToLower();

            switch (format) {
                case "json":
                    return await LearnJson(input, data);

                case "yaml":
                    return await LearnYaml(input, data);

                default:
                    return await LearnYaml(input, data);
            }

        }

        [Parser(@"(?:(?:,\s*)*([^,]+)\s+?(?:is|=)\s+?([^,]+))+")]
        public async Task<bool> ParserPropUpdate(Match match) {
            var reply = _learnService.UpdateMatches(match).Build();
            await ReplyAsync(embed: reply);
            return true;
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?\n(.*)")]
        public async Task<bool> ParserLearnEmbedUnquoted(string input, string yaml) {
            var embed = await LearnYaml(input, yaml);

            if (embed != null) {
                await ReplyAsync(embed: embed);
                return true;
            }

            return false;
        }

        [Parser(@"(.*)")]
        public async Task<bool> GlobalLookup(string input) {
            var entry = _learnService.Lookup(input);

            if (entry != null) {
                await ReplyAsync(embed: entry.AsEmbed());
                return true;
            }

            return false;
        }

    }

}