using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using BindingAttributes;

using Discord;

using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Models;
using Disunity.Disinfo.Models.Entities;
using Disunity.Disinfo.Modules;
using Disunity.Disinfo.Services.Singleton;


namespace Disunity.Disinfo.Services.Scoped {

    [AsScoped]
    public class LearnModuleService {

        private readonly ContextService _contextService;
        private readonly EmbedService _embeds;
        private readonly LearnModuleParserService _parserService;
        private readonly LearnModuleFilterService _filterService;

        public LearnModuleService(ContextService contextService, 
                                  EmbedService embeds,
                                  LearnModuleParserService parserService, 
                                  LearnModuleFilterService filterService) {
            _contextService = contextService;
            _embeds = embeds;
            _parserService = parserService;
            _filterService = filterService;

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

        public Embed LearnJson(string index, string json) {
            var data = _parserService.LoadJson(index, json);
            var fact = _embeds.Update(data);
            return fact.AsEmbed();
        }

        public Embed LearnYaml(string input, string yaml) {
            var data = _parserService.LoadYaml(input, yaml);
            var result = _embeds.Update(data);
            return result.AsEmbed();
        }

        public ReportBuilder ForgetMatches(Match match) {
            var refs = _parserService.ParseCaptures(match);
            var filterResult = _filterService.FilterRefs(refs);
            var (empty, nonEmpty) = _filterService.EmptyOrUpdated("null", filterResult.Unlocked);
            var deleted = DeleteRefs(empty);

            return new ReportBuilder()
                   .WithDeletedRefs(deleted)
                   .WithGlobalRefs(filterResult.Global)
                   .WithLockedRefs(filterResult.Locked)
                   .WithMissingRefs(filterResult.Unknown)
                   .WithUpdatedRefs(nonEmpty);

        }

        public ReportBuilder UpdateMatches(Match match) {
            var refs = _parserService.ParseCaptures(match);
            var vals = _parserService.CapturesFrom(match, 2);
            
            var map = refs.Zip(vals, (k, v) => new {k.Input, v})
                          .ToDictionary(x => x.Input, x => x.v);

            var filterResult = _filterService.FilterRefs(refs);
            var (empty, nonEmpty) = _filterService.EmptyOrUpdated(map, filterResult.Unlocked);
            var deleted = DeleteRefs(empty);
            
            var (skipped, created) = filterResult.Unknown.Select(r => {
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

            return new ReportBuilder()
                   .WithCreatedRefs(created)
                   .WithDeletedRefs(deleted)
                   .WithUpdatedRefs(nonEmpty)
                   .WithGlobalRefs(filterResult.Global)
                   .WithLockedRefs(filterResult.Locked)
                   .WithSkippedRefs(skipped);
        }
        
    }

}