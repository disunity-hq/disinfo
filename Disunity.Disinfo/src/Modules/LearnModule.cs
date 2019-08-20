using System.Linq;
using System.Threading.Tasks;

using Discord;
using Discord.Commands;

using Disunity.Disinfo.Services;

using Microsoft.Extensions.Configuration;

using CommandService = Discord.Commands.CommandService;


namespace Disunity.Disinfo.Modules {

    public class Fact : ITable {

        public string Id { get; set; }
        public string Definition { get; set; }

    }

    public class LearnModule : ModuleBase<SocketCommandContext> {

        private readonly CommandService _service;
        private readonly IConfigurationRoot _config;
        private readonly DbService _dbService;
        private readonly string _prefix;

        public LearnModule(CommandService service, IConfigurationRoot config, DbService dbService) {
            _service = service;
            _config = config;
            _dbService = dbService;
            _prefix = _config["Prefix"];
        }

        [Command("learn")]
        public async Task LearnAsync(string word, string isWord, [Remainder] string definition) {
            if (isWord != "is") {
                await ReplyAsync($"Did you mean '{word} **is** {isWord} {definition}'?");
                return;
            }

            _dbService.WithTable<Fact>("facts", (t) => {
                var fact = new Fact() {
                    Id = word,
                    Definition = definition,
                };

                t.Insert(fact);
            });

            await ReplyAsync($"OK, `{word}` is '{definition}'.");
        }
        
        [Command("what is")]
        public async Task WhatAsync(string query) {

            string answer = null;

            _dbService.WithTable<Fact>("facts", (t) => {
                var fact = t.FindById(query);

                if (fact != null) {
                    answer = fact.Definition;
                }
                
            });

            if (answer == null) {
                await ReplyAsync("I have no idea, sorry");
            } else {
                await ReplyAsync($"`{query}` is {answer}");
            }
            
        }

    }

}