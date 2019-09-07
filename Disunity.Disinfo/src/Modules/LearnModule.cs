using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Attributes;
using Disunity.Disinfo.Services;
using Disunity.Disinfo.Services.Scoped;
using Disunity.Disinfo.Services.Singleton;

using EmbedDB.Data;
using EmbedDB.Entities;

using Newtonsoft.Json;

using SharpYaml;


namespace Disunity.Disinfo.Modules {

    [AsScoped]
    public class LearnModule : ModuleBase<SocketCommandContext> {

        private readonly ContextService _contextService;
        private readonly EmbedService _embeds;
        private readonly LearnModuleService _learnService;
        private readonly ClientService _clientService;
        private readonly EmbedDBContext _dbContext;

        public LearnModule(ContextService contextService,
                           EmbedService embeds,
                           LearnModuleService learnService, 
                           ClientService clientService,
                           EmbedDBContext dbContext) {
            _contextService = contextService;
            _embeds = embeds;
            _learnService = learnService;
            _clientService = clientService;
            _dbContext = dbContext;
        }

        public Task ReplyAsync(string message = null, Embed embed = null) {
            return _contextService.Context.Channel.SendMessageAsync(message, embed: embed);
        }

        private async Task HandleArgumentError(ArgumentException e) {
            var reply = new EmbedBuilder()
                        .WithTitle($"Property error: {e.ParamName}")
                        .WithDescription(e.ParamName)
                        .Build();

            await ReplyAsync(embed: reply);
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

        public async Task<bool> LearnJson(string input, string json) {
            try {
                var reply = _learnService.LearnJson(input, json);
                if (reply == null) return false;
                await ReplyAsync(embed: reply.Build());
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
            }
            catch (ArgumentException e) {
                await HandleArgumentError(e);
            }

            return true;

        }

        public async Task<bool> LearnYaml(string input, string yaml) {
            try {
                var reply = _learnService.LearnYaml(input, yaml);
                if (reply == null) return false;
                await ReplyAsync(embed: reply.Build());
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
            }
            catch (ArgumentException e) {
                await HandleArgumentError(e);
            }

            return true;
        }

        [Parser(@"^(?i)inspect")]
        public async Task<bool> InspectAsync() {
            if (!_contextService.IsManagement) return false;
            
            var entries = _embeds.All();

            if (entries.Count() == 0) {
                await ReplyAsync("No information found.");
                return true;
            }

            var fields = new List<EmbedFieldBuilder>() {
                new EmbedFieldBuilder()
                    .WithName("Number of entries")
                    .WithValue(entries.Count())
            };

            var byGuild = entries.Select(x => (Guild: _clientService.Client.GetGuild(ulong.Parse(x.Guild)).Name, x.Slug))
                                 .GroupBy(x => x.Guild)
                                 .ToDictionary(o => o.Key, o => o.ToList());

            foreach (var (guild, mappedEntries) in byGuild) {
                fields.Add(new EmbedFieldBuilder()
                           .WithName(guild)
                           .WithValue(string.Join(", ", mappedEntries.Select(x => x.Slug))));
            }

            var reply = new EmbedBuilder()
                        .WithTitle("Global info")
                        .WithFields(fields);

            await ReplyAsync(embed: reply.Build());
            return true;
        }

        [Parser(@"^(?i)inspect (\d+)")]
        public async Task<bool> InspectAsync(string guild) {
            var entries = _embeds.AllForGuild(guild).ToArray();

            if (entries.Any()) {
                await ReplyAsync("No information found.");
                return true;
            }

            var reply = new EmbedBuilder()
                        .WithTitle($"Info about guild #{guild}")
                        .WithFields(new EmbedFieldBuilder().WithName("Number of entries").WithValue(entries.Count()));

            await ReplyAsync(embed: reply.Build());
            return true;
        }

        // forget <fact|prop>, <fact|prop>, ...
        [Parser(@"^(?i)forget (?:(?:,\s*)*([^,]+))*")]
        public async Task<bool> ParserForget(Match match) {
            var reply = _learnService.ForgetMatches(match).Build();
            await ReplyAsync(embed: reply);
            return true;
        }

        [Parser(@"(.*?)\s+?(?:=)\s+?```(.*?)\n(.*)```")]
        public async Task<bool> ParserLearnEmbed(string input, string format, string data) {
            if (!await CheckAccess(input)) return true;

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

        [Parser(@"(.*?)\s+?(?:=)\s+?\n(.*)")]
        public async Task<bool> ParserLearnEmbedUnquoted(string input, string yaml) {
            return await LearnYaml(input, yaml);
        }

        [Parser(@"(?:(?:,\s*)*([^,]+)\s+?(?:=)\s+?([^,]+))+")]
        public async Task<bool> ParserPropUpdate(Match match) {
            try {
                var reply = _learnService.UpdateMatches(match).Build();
                await ReplyAsync(embed: reply);
            }
            catch (ArgumentException e) {
                await HandleArgumentError(e);
            }

            return true;
        }

        [Parser(@"(.*)")]
        public async Task<bool> GlobalLookup(string input) {
            

            Console.WriteLine("SAVING EMBED ENTRY");
            var entity = new EmbedEntity();
            entity.GuildId = 0;
            entity.Slug = "disinfo";
            entity.Description = "Hello World";
            entity.Title = "Disinfo";
            _dbContext.SaveChanges();

            
            var entry = _learnService.Lookup(input);

            if (entry != null) {
                await ReplyAsync(embed: entry.AsEmbed());
                return true;
            }

            return false;
        }

    }

}