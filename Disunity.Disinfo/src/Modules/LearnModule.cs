using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Attributes;
using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Models;
using Disunity.Disinfo.Models.Entities;
using Disunity.Disinfo.Services;
using Disunity.Disinfo.Services.Scoped;
using Disunity.Disinfo.Services.Singleton;

using Newtonsoft.Json;

using SharpYaml;
using SharpYaml.Serialization;


namespace Disunity.Disinfo.Modules {

    [AsScoped]
    public class LearnModule : ModuleBase<SocketCommandContext> {

        private readonly ContextService _contextService;
        private readonly EmbedService _embeds;
        private readonly RoleService _roles;
        private readonly LearnModuleParserService _parserService;
        private readonly LearnModuleFilterService _filterService;
        private readonly LearnModuleService _learnService;
        
        public LearnModule(ContextService contextService, 
                           EmbedService embeds, 
                           RoleService roles, 
                           LearnModuleParserService parserService, 
                           LearnModuleFilterService filterService, 
                           LearnModuleService learnService) {
            _contextService = contextService;
            _embeds = embeds;
            _roles = roles;
            _parserService = parserService;
            _filterService = filterService;
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
                return _learnService.ParserLearnJson(input, json);
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
                return _learnService.ParserLearnYaml(input, yaml);
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
            var refs = _parserService.ParseCaptures(match);
            var filterResult = _filterService.FilterRefs(refs);

            var (factRefs, propRefs) = _filterService.FactOrProperty(filterResult.Unlocked);
            var missingRefs = filterResult.Unknown.Concat(filterResult.Unknown);
            var (emptyFacts, updatedFacts) = _filterService.EmptyOrUpdated("null", filterResult.Known);
            var deletedRefs = _learnService.DeleteRefs(factRefs.Concat(emptyFacts));

            var reply = new ReportBuilder()
                        .WithDeletedRefs(deletedRefs)
                        .WithGlobalRefs(filterResult.Global)
                        .WithLockedRefs(filterResult.Global)
                        .WithMissingRefs(missingRefs)
                        .WithUpdatedRefs(updatedFacts)
                        .Build();

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
            var refs = _parserService.ParseCaptures(match);
            var vals = _parserService.CapturesFrom(match, 2);

            var map = refs.Zip(vals, (k, v) => new {k.Input, v})
                          .ToDictionary(x => x.Input, x => x.v);

            var filteredRefs = _filterService.FilterRefs(refs);
            // after updating some facts will be empty
            var (emptyRefs, updatedRefs) = _filterService.EmptyOrUpdated(map, filteredRefs.Unlocked);
            var deletedRefs = _learnService.DeleteRefs(emptyRefs);

            // we can create unknown facts
            var (skippedRefs, createdRefs) = filteredRefs.Unknown.Select(r => {
                var value = map[r.Input];

                if (value == "null") {
                    r.EmbedEntry = null;
                    return r;
                }

                r.EmbedEntry = r.Property == null
                    ? _embeds.Learn(r.Slug, value, _contextService.Guild)
                    : _embeds.Update(r.Slug, r.Property, value, _contextService.Guild);

                return r;

            }).Fork(r => r.EmbedEntry == null);

            var reply = new ReportBuilder()
                        .WithCreatedRefs(createdRefs)
                        .WithDeletedRefs(deletedRefs)
                        .WithUpdatedRefs(updatedRefs)
                        .WithGlobalRefs(filteredRefs.Global)
                        .WithLockedRefs(filteredRefs.Locked)
                        .WithSkippedRefs(skippedRefs)
                        .Build();

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