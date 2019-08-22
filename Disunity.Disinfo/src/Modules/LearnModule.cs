using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Services;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SharpYaml.Serialization;

using Slugify;


namespace Disunity.Disinfo.Modules {

    public class LearnModule : ModuleBase<SocketCommandContext> {

        private readonly FactService _facts;
        private Serializer _serializer;

        public LearnModule(FactService facts) {
            _facts = facts;
            _serializer = new SharpYaml.Serialization.Serializer();
        }

        private Task ReplyAsync(ICommandContext context, string message = null, Embed embed = null) {
            return context.Channel.SendMessageAsync(message, embed: embed);
        }

//        [Command("forget")]
//        public async Task ForgetAsync([Remainder] string input) {
//            var forgot = _facts.Forget(input);
//
//            if (forgot) {
//                await ReplyAsync($"OK, I forgot about it.");
//            } else {
//                await ReplyAsync($"Didn't know what it was anyway.");
//            }
//            
//        }

        [Parser(@"[Ff][Oo][Rr][Gg][Ee][Tt] (?:([^,.]+)\.([^,.]+),)*(?:([^,.]+)\.([^,.]+))+")]
        public async Task<bool> ParserForget(ICommandContext context, string index, string prop) {
            return await ParserPropUpdate(context, index, prop, "null");
        }

        [Parser(@"([^.]*)\.([^.]*)\s+?(?:is|=)\s+?(.*)")]
        public async Task<bool> ParserPropUpdate(ICommandContext context, string input, string prop, string value) {
            var fact = _facts.Update(input, prop, value);
            await ReplyAsync(context, embed: fact.AsEmbed());
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

        [Parser(@"(.*?)\s+?(?:is|=)\s+?(.*)")]
        public async Task<bool> ParserLearn(ICommandContext context, string input, string definition) {
            var fact = _facts.Learn(input, definition);
            await ReplyAsync(context, "OK, I'll remember:", fact.AsEmbed());
            return true;
        }

        [Parser(@"(.*?)\?")]
        public async Task<bool> ParserLookup(ICommandContext context, string input) {
            var fact = _facts.Lookup(input);

            if (fact == null) {
                await ReplyAsync(context, "I have no idea, sorry");
            } else {
                await ReplyAsync(context, embed: fact.AsEmbed());
            }

            return true;
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