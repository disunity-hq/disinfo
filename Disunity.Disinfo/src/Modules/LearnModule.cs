using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Attributes;
using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Services;

using Newtonsoft.Json;

using SharpYaml;
using SharpYaml.Serialization;

using Embed = Disunity.Disinfo.Models.Embed;


namespace Disunity.Disinfo.Modules {

    public class FactRef {

        public Embed Embed { get; set; }
        public string PropStr { get; set; }
        public string InputStr { get; set; }
        public string FactStr { get; set; }

    }

    [AsScoped]
    public class LearnModule : ModuleBase<SocketCommandContext> {

        private readonly ContextService _contextService;
        private readonly EmbedService _embeds;
        private readonly RoleService _roles;
        private readonly Serializer _serializer;

        private static readonly string[] _validFields = {
            "description", "author", "color", "url", "image", "thumbnail", "locked"
        };

        public LearnModule(ContextService contextService, EmbedService embeds, RoleService roles) {
            _contextService = contextService;
            _embeds = embeds;
            _roles = roles;
            _serializer = new Serializer();
        }

        private Task ReplyAsync(string message = null, Discord.Embed embed = null) {
            return _contextService.Context.Channel.SendMessageAsync(message, embed: embed);
        }

        private ImmutableArray<string> CapturesFrom(Match match, int index = 1) {
            if (index >= match.Groups.Count) {
                return new ImmutableArray<string>();
            }

            return match.Groups[index]
                        .Captures
                        .Select(c => c.Value.Trim())
                        .ToImmutableArray();
        }

        private Embed ForgetProperty(string index, bool isAdmin = false) {
            var parts = index.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
            var fact = _embeds.Lookup(parts[0]);

            if (fact.Locked && !isAdmin) {
                return null;
            }

            return _embeds.Update(parts[0], parts[1], "null");
        }

        private Embed ForgetFact(string index, bool isAdmin = false) {
            var fact = _embeds.Lookup(index);

            if (fact.Locked && !isAdmin) {
                return null;
            }

            return _embeds.Forget(index) ? fact : null;
        }

        private FactRef ParseRef(string input) {
            var (factStr, propStr, _) = input.Split('.', 2);
            var fact = _embeds.Lookup(factStr);

            return new FactRef {
                Embed = fact,
                FactStr = factStr,
                PropStr = propStr,
                InputStr = input
            };
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) FactExists(IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.Embed != null);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) FactIsLocked(bool isAdmin, IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.Embed.Locked && !isAdmin);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) FactOrProperty(IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.PropStr == null);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) PropertyIsValid(IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.Embed.Fields.ContainsKey(r.PropStr) || _validFields.Contains(r.PropStr));
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) EmptyOrUpdated(string value, IEnumerable<FactRef> refs) {
            return refs.Select(r => {
                r.Embed = _embeds.Update(r.FactStr, r.PropStr, value);
                return r;
            }).Fork(r => r.Embed.IsEmpty);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) EmptyOrUpdated(
            Dictionary<string, string> map, IEnumerable<FactRef> refs) {
            return refs.Select(r => {
                var value = map[r.InputStr];
                r.Embed = _embeds.Update(r.FactStr, r.PropStr, value);
                return r;
            }).Fork(r => r.Embed.IsEmpty);
        }

        private IEnumerable<FactRef> DeleteRefs(IEnumerable<FactRef> refs) {
            return refs.Select(r => {
                _embeds.Forget(r.FactStr);
                return r;
            });
        }

        private IEnumerable<FactRef> ParseCaptures(Match match, int index = 1) {
            var captures = CapturesFrom(match, index);
            return captures.Select(ParseRef).ToList();
        }

        // forget <fact|prop>, <fact|prop>, ...
        [Parser(@"^(?i)forget (?:(?:,\s*)*([^,]+))*")]
        public async Task<bool> ParserForget(Match match) {
            var isAdmin = _contextService.Context.IsAdmin();
            var refs = ParseCaptures(match);
            var (knownRefs, unknownRefs) = FactExists(refs);
            var (lockedRefs, unlockedRefs) = FactIsLocked(isAdmin, knownRefs);
            var (factRefs, propRefs) = FactOrProperty(unlockedRefs);
            var (knownPropRefs, unknownPropRefs) = PropertyIsValid(propRefs);
            var missingRefs = unknownRefs.Concat(unknownPropRefs);
            var (emptyFacts, updatedFacts) = EmptyOrUpdated("null", knownPropRefs);
            var deletedRefs = DeleteRefs(factRefs.Concat(emptyFacts));

            var reply = new ReportBuilder()
                        .WithDeletedRefs(deletedRefs)
                        .WithLockedRefs(lockedRefs)
                        .WithMissingRefs(missingRefs)
                        .WithUpdatedRefs(updatedFacts)
                        .Build();

            await ReplyAsync(embed: reply);

            return true;
        }

        private async Task<bool> ParserLearnJson(string index, string data) {
            Dictionary<string, string> factData;

            try {
                factData = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

                if (factData == null) {
                    throw new JsonException("JSON data was blank or not a well-formed Object.");
                }

                factData["Id"] = index;
                var fact = _embeds.Update(factData);
                await ReplyAsync(embed: fact.AsEmbed());
                return true;

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
                return true;
            }
        }

        private async Task<bool> ParserLearnYaml(string index, string data) {
            Dictionary<string, string> factData;

            try {
                factData = _serializer.Deserialize<Dictionary<string, string>>(data);

                if (factData == null) {
                    throw new YamlException("YAML data was blank or not a well-formed Object.");
                }

                factData["Id"] = index;
                var fact = _embeds.Update(factData);
                await ReplyAsync(embed: fact.AsEmbed());
                return true;
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
                return true;
            }
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?```(.*?)\n(.*)```")]
        public async Task<bool> ParserLearnEmbed(string index, string format, string data) {
            format = format.ToLower();

            switch (format) {
                case "json":
                    return await ParserLearnJson(index, data);

                case "yaml":
                    return await ParserLearnYaml(index, data);

                default:
                    return false;
            }

        }

        [Parser(@"(?:(?:,\s*)*([^,]+)\s+?(?:is|=)\s+?([^,]+))+")]
        public async Task<bool> ParserPropUpdate(Match match) {
            var isAdmin = _contextService.Context.IsAdmin();
            var refs = ParseCaptures(match);
            var vals = CapturesFrom(match, 2);

            var map = refs.Zip(vals, (k, v) => new {k.InputStr, v})
                          .ToDictionary(x => x.InputStr, x => x.v);

            // refs will either exist already or not
            var (knownRefs, unknownRefs) = FactExists(refs);
            // some known refs will be locked to the user
            var (lockedRefs, unlockedRefs) = FactIsLocked(isAdmin, knownRefs);
            // after updating some facts will be empty
            var (emptyRefs, updatedRefs) = EmptyOrUpdated(map, unlockedRefs);
            var deletedRefs = DeleteRefs(emptyRefs);

            // we can create unknown facts
            var (skippedRefs, createdRefs) = unknownRefs.Select(r => {
                var value = map[r.InputStr];

                if (value == "null") {
                    r.Embed = null;
                    return r;
                }

                r.Embed = r.PropStr == null
                    ? _embeds.Learn(r.FactStr, value)
                    : _embeds.Update(r.FactStr, r.PropStr, value);

                return r;

            }).Fork(r => r.Embed == null);

            var reply = new ReportBuilder()
                        .WithCreatedRefs(createdRefs)
                        .WithDeletedRefs(deletedRefs)
                        .WithUpdatedRefs(updatedRefs)
                        .WithLockedRefs(lockedRefs)
                        .WithSkippedRefs(skippedRefs)
                        .Build();

            await ReplyAsync(embed: reply);
            return true;
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?```(.*)```")]
        public async Task<bool> ParserLearnEmbed(string input, string json) {
            Dictionary<string, string> factData = null;

            try {
                factData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (JsonException e) {
                try {
                    factData = _serializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch (YamlException) {
                    return false;
                }
            }

            if (factData != null) {
                factData["Id"] = input;
                var fact = _embeds.Update(factData);
                await ReplyAsync(embed: fact.AsEmbed());
                return true;
            }

            return false;
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?\n(.*)")]
        public async Task<bool> ParserLearnEmbedUnquoted(string input, string json) {
            return await ParserLearnEmbed(input, json);
        }

        [Parser(@"(.*)")]
        public async Task<bool> GlobalLookup(string input) {
            var fact = _embeds.Lookup(input);

            if (fact != null) {
                await ReplyAsync(embed: fact.AsEmbed());
                return true;
            }

            return false;

        }

    }

}