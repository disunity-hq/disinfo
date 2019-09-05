using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using BindingAttributes;

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

        public ReportBuilder LearnJson(string input, string json) {
            var refs = _parserService.LoadJson(input, json);
            return UpdateMapping(refs);
        }

        public ReportBuilder LearnYaml(string input, string yaml) {
            var refs = _parserService.LoadYaml(input, yaml);
            return UpdateMapping(refs);
        }

        public ReportBuilder ForgetMatches(Match match) {
            var refs = _parserService.ParseReferences(match);
            var filterResult = _filterService.FilterRefs(refs);

            var (deleted, nonEmpty) = filterResult.Valid.Select(r => {
                _embeds.UpdateReference(r, "null", r.EmbedEntry.Guild);
                return r;
            }).Fork(r => r.EmbedEntry == null);

            return new ReportBuilder()
                   .WithDeletedRefs(deleted)
                   .WithGlobalRefs(filterResult.Global)
                   .WithLockedRefs(filterResult.Locked)
                   .WithMissingRefs(filterResult.Invalid)
                   .WithUpdatedRefs(nonEmpty.Where(r => !deleted.Any(r2 => r.EmbedEntry.Id != r2.EmbedEntry.Id)));
        }

        public ReportBuilder UpdateMapping(Dictionary<EmbedReference, string> map) {
            var filterResult = _filterService.FilterRefs(map.Keys);

            var (deleted, updated) = filterResult.Valid.Select(r => {
                _embeds.UpdateReference(r, map[r], r.EmbedEntry.Guild);
                return r;
            }).Fork(r => r.EmbedEntry == null);

            var (skipped, created) = filterResult.Invalid.Select(r => {
                _embeds.UpdateReference(r, map[r], _contextService.Guild);
                return r;
            }).Fork(r => r.EmbedEntry == null);

            return new ReportBuilder()
                   .WithCreatedRefs(created)
                   .WithDeletedRefs(deleted)
                   .WithUpdatedRefs(updated)
                   .WithGlobalRefs(filterResult.Global)
                   .WithLockedRefs(filterResult.Locked)
                   .WithSkippedRefs(skipped);    
        }

        public ReportBuilder UpdateMatches(Match match) {
            var refs = _parserService.ParseReferences(match);
            var values = _parserService.CapturesFromMatchGroup(match, 2);

            var map = refs.Zip(values, (k, v) => new {k, v})
                          .ToDictionary(x => x.k, x => x.v);

            return UpdateMapping(map);
        }

    }

}