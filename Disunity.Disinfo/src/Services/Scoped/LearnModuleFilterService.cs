using System.Collections.Generic;
using System.Linq;

using BindingAttributes;

using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Models;
using Disunity.Disinfo.Services.Singleton;


namespace Disunity.Disinfo.Services.Scoped {

    [AsScoped]
    public class LearnModuleFilterService {

        private readonly EmbedService _embeds;
        private readonly ContextService _contextService;
        private static readonly string[] _validFields = {
            "description", "author", "color", "url", "image", "thumbnail", "locked"
        };
        

        public LearnModuleFilterService(EmbedService embeds, ContextService contextService) {
            _embeds = embeds;
            _contextService = contextService;
        }

        public (IEnumerable<EmbedRef>, IEnumerable<EmbedRef>) FactExists(IEnumerable<EmbedRef> refs) {
            return refs.Fork(r => _embeds.Query(r.Slug, _contextService.Guild) != null);
        }

        public (IEnumerable<EmbedRef>, IEnumerable<EmbedRef>) FactIsGlobal(IEnumerable<EmbedRef> refs) {
            return refs.Fork(r => r.EmbedEntry.Guild == "0" && !_contextService.IsManagement);
        }

        public (IEnumerable<EmbedRef>, IEnumerable<EmbedRef>) FactIsLocked(IEnumerable<EmbedRef> refs) {
            return refs.Fork(r => r.EmbedEntry.Locked && !_contextService.IsAdmin);
        }

        public (IEnumerable<EmbedRef>, IEnumerable<EmbedRef>) FactOrProperty(IEnumerable<EmbedRef> refs) {
            return refs.Fork(r => r.Property == null);
        }

        public (IEnumerable<EmbedRef>, IEnumerable<EmbedRef>) PropertyIsValid(IEnumerable<EmbedRef> refs) {
            return refs.Fork(r => r.EmbedEntry.Fields.ContainsKey(r.Property) || _validFields.Contains(r.Property));
        }

        public (IEnumerable<EmbedRef>, IEnumerable<EmbedRef>) EmptyOrUpdated(string value, IEnumerable<EmbedRef> refs) {
            return refs.Select(r => {
                r.EmbedEntry = _embeds.Update(r.Slug, r.Property, value, r.EmbedEntry?.Guild);
                return r;
            }).Fork(r => r.EmbedEntry.IsEmpty);
        }

        public (IEnumerable<EmbedRef>, IEnumerable<EmbedRef>) EmptyOrUpdated(
            Dictionary<string, string> map, IEnumerable<EmbedRef> refs) {
            return refs.Select(r => {
                var value = map[r.Input];
                r.EmbedEntry = _embeds.Update(r.Slug, r.Property, value);
                return r;
            }).Fork(r => r.EmbedEntry.IsEmpty);
        }

        public FilterResults FilterRefs(IEnumerable<EmbedRef> refs) {
            // refs will either exist already or not
            var (Known, Unknown) = FactExists(refs);
            var (Global, Local) = FactIsGlobal(Known);
            // some known refs will be locked to the user
            var (Locked, Unlocked) = FactIsLocked(Local);
            var (Slugs, Properties) = FactOrProperty(Unlocked);

            return new FilterResults(
                Known, Unknown, Global, Local, Locked, Unlocked, Slugs, Properties);
        }


    }

}