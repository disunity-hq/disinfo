using System.Collections.Generic;
using System.Linq;

using BindingAttributes;

using Discord;

using Disunity.Disinfo.Models;
using Disunity.Disinfo.Models.Entities;
using Disunity.Disinfo.Services.Singleton;


namespace Disunity.Disinfo.Services.Scoped {

    [AsScoped]
    public class LearnModuleService {

        private readonly ContextService _contextService;
        private readonly EmbedService _embeds;
        private readonly LearnModuleParserService _parserService;

        public LearnModuleService(ContextService contextService, EmbedService embeds,
                                  LearnModuleParserService parserService) {
            _contextService = contextService;
            _embeds = embeds;
            _parserService = parserService;

        }

        public EmbedEntry Lookup(string input) {
            var guild = _contextService.Guild;
            var entry = _embeds.Query(input);

            if (guild != "0" && entry == null) {
                entry = _embeds.Query(input, guild);
            }

            return entry;
        }

        public IEnumerable<EmbedRef> DeleteRefs(IEnumerable<EmbedRef> refs) {
            return refs.Select(r => {
                _embeds.Forget(r.Slug, _contextService.Guild);
                return r;
            });
        }

        public Embed ParserLearnJson(string index, string json) {
            var data = _parserService.LoadJson(index, json);
            var fact = _embeds.Update(data);
            return fact.AsEmbed();
        }

        public Embed ParserLearnYaml(string input, string yaml) {
            Dictionary<string, string> factData;
            var data = _parserService.LoadYaml(input, yaml);
            var result = _embeds.Update(data);
            return result.AsEmbed();
        }
        
    }

}