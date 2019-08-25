using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using BindingAttributes;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Services;

using LiteDB;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SharpYaml.Serialization;

using Slugify;


namespace Disunity.Disinfo.Modules {

    public class FactRef {

        public Fact Fact { get; set; }
        public string PropStr { get; set; }
        public string InputStr { get; set; }
        public string FactStr { get; set; }

    }

    [AsSingleton]
    public class LearnModule : ModuleBase<SocketCommandContext> {

        private readonly FactService _facts;
        private readonly RoleService _roles;
        private readonly Serializer _serializer;

        private static readonly string[] _validFields = {
            "description", "author", "color", "url", "image", "thumbnail", "locked"
        };

        public LearnModule(FactService facts, RoleService roles) {
            _facts = facts;
            _roles = roles;
            _serializer = new Serializer();
        }

        private Task ReplyAsync(ICommandContext context, string message = null, Embed embed = null) {
            return context.Channel.SendMessageAsync(message, embed: embed);
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

        private Fact ForgetProperty(string index, bool isAdmin = false) {
            var parts = index.Split('.', 2, StringSplitOptions.RemoveEmptyEntries);
            var fact = _facts.Lookup(parts[0]);

            if (fact.Locked && !isAdmin) {
                return null;
            }

            return _facts.Update(parts[0], parts[1], "null");
        }

        private Fact ForgetFact(string index, bool isAdmin = false) {
            var fact = _facts.Lookup(index);

            if (fact.Locked && !isAdmin) {
                return null;
            }

            return _facts.Forget(index) ? fact : null;
        }

        private FactRef ParseRef(string input) {
            var (factStr, propStr, _) = input.Split('.', 2);
            var fact = _facts.Lookup(factStr);

            return new FactRef {
                Fact = fact,
                FactStr = factStr,
                PropStr = propStr,
                InputStr = input
            };
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) FactExists(IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.Fact != null);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) FactIsLocked(bool isAdmin, IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.Fact.Locked && !isAdmin);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) FactOrProperty(IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.PropStr == null);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) PropertyIsValid(IEnumerable<FactRef> refs) {
            return refs.Fork(r => r.Fact.Fields.ContainsKey(r.PropStr) || _validFields.Contains(r.PropStr));
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) EmptyOrUpdated(string value, IEnumerable<FactRef> refs) {
            return refs.Select(r => {
                r.Fact = _facts.Update(r.FactStr, r.PropStr, value);
                return r;
            }).Fork(r => r.Fact.IsEmpty);
        }

        private (IEnumerable<FactRef>, IEnumerable<FactRef>) EmptyOrUpdated(
            Dictionary<string, string> map, IEnumerable<FactRef> refs) {
            return refs.Select(r => {
                var value = map[r.InputStr];
                r.Fact = _facts.Update(r.FactStr, r.PropStr, value);
                return r;
            }).Fork(r => r.Fact.IsEmpty);
        }

        private IEnumerable<FactRef> DeleteRefs(IEnumerable<FactRef> refs) {
            return refs.Select(r => {
                _facts.Forget(r.FactStr);
                return r;
            });
        }

        private IEnumerable<FactRef> ParseCaptures(Match match, int index = 1) {
            var captures = CapturesFrom(match, index);
            return captures.Select(ParseRef).ToList();
        }

        // forget <fact|prop>, <fact|prop>, ...
        [Parser(@"^(?i)forget (?:(?:,\s*)*([^,]+))*")]
        public async Task<bool> ParserForget(ICommandContext context, Match match) {
            var isAdmin = context.IsAdmin();
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

            await ReplyAsync(context, embed: reply);

            return true;
        }

        private async Task<bool> ParserLearnJson(ICommandContext context, string index, string data) {
            Dictionary<string, string> factData;

            try {
                factData = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);

                if (factData == null) {
                    throw new JsonException("JSON data was blank or not a well-formed Object.");
                }

                factData["Id"] = index;
                var fact = _facts.Update(factData);
                await ReplyAsync(context, embed: fact.AsEmbed());
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

                await ReplyAsync(context, embed: reply);
                return true;
            }
        }

        private async Task<bool> ParserLearnYaml(ICommandContext context, string index, string data) {
            Dictionary<string, string> factData;

            try {
                factData = _serializer.Deserialize<Dictionary<string, string>>(data);

                if (factData == null) {
                    throw new SharpYaml.YamlException("YAML data was blank or not a well-formed Object.");
                }

                factData["Id"] = index;
                var fact = _facts.Update(factData);
                await ReplyAsync(context, embed: fact.AsEmbed());
                return true;
            }
            catch (SharpYaml.YamlException e) {
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

                await ReplyAsync(context, embed: reply);
                return true;
            }
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?```(.*?)\n(.*)```")]
        public async Task<bool> ParserLearnEmbed(ICommandContext context, string index, string format, string data) {
            format = format.ToLower();

            switch (format) {
                case "json":
                    return await ParserLearnJson(context, index, data);

                case "yaml":
                    return await ParserLearnYaml(context, index, data);

                default:
                    return false;
            }

        }

        [Parser(@"(?:(?:,\s*)*([^,]+)\s+?(?:is|=)\s+?([^,]+))+")]
        public async Task<bool> ParserPropUpdate(ICommandContext context, Match match) {
            var isAdmin = context.IsAdmin();
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
                    r.Fact = null;
                    return r;
                }

                r.Fact = r.PropStr == null
                    ? _facts.Learn(r.FactStr, value)
                    : _facts.Update(r.FactStr, r.PropStr, value);

                return r;

            }).Fork(r => r.Fact == null);

            var reply = new ReportBuilder()
                        .WithCreatedRefs(createdRefs)
                        .WithDeletedRefs(deletedRefs)
                        .WithUpdatedRefs(updatedRefs)
                        .WithLockedRefs(lockedRefs)
                        .WithSkippedRefs(skippedRefs)
                        .Build();

            await ReplyAsync(context, embed: reply);
            return true;
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?```(.*)```")]
        public async Task<bool> ParserLearnEmbed(ICommandContext context, string input, string json) {
            Dictionary<string, string> factData = null;

            try {
                factData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }
            catch (JsonException e) {
                try {
                    factData = _serializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch (SharpYaml.YamlException) {
                    return false;
                }
            }

            if (factData != null) {
                factData["Id"] = input;
                var fact = _facts.Update(factData);
                await ReplyAsync(context, embed: fact.AsEmbed());
                return true;
            }

            return false;
        }

        [Parser(@"(.*?)\s+?(?:is|=)\s+?\n(.*)")]
        public async Task<bool> ParserLearnEmbedUnquoted(ICommandContext context, string input, string json) {
            return await ParserLearnEmbed(context, input, json);
        }

        [Parser(@"(.*)")]
        public async Task<bool> GlobalLookup(ICommandContext context, string input) {
            var fact = _facts.Lookup(input);

            if (fact != null) {
                await ReplyAsync(context, embed: fact.AsEmbed());
                return true;
            }

            return false;

        }

    }

}