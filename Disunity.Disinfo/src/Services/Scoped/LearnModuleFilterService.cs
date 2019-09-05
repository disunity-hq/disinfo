using System;
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
            "title", "description", "author", "color", "url", "image", "thumbnail", "locked"
        };

        public LearnModuleFilterService(EmbedService embeds, ContextService contextService) {
            _embeds = embeds;
            _contextService = contextService;
        }

        public (IEnumerable<EmbedReference>, IEnumerable<EmbedReference>) FactExists(IEnumerable<EmbedReference> refs) {
            return refs.Fork(r => r.EmbedEntry != null);
        }

        public (IEnumerable<EmbedReference>, IEnumerable<EmbedReference>) FactIsGlobal(IEnumerable<EmbedReference> refs) {
            return refs.Fork(r => r.EmbedEntry.Guild == "0" && !_contextService.IsManagement);
        }

        public (IEnumerable<EmbedReference>, IEnumerable<EmbedReference>) FactIsLocked(IEnumerable<EmbedReference> refs) {
            return refs.Fork(r => r.EmbedEntry.Locked && !_contextService.IsAdmin);
        }

        public (IEnumerable<EmbedReference>, IEnumerable<EmbedReference>) FactOrProperty(IEnumerable<EmbedReference> refs) {
            return refs.Fork(r => r.Property == null);
        }

        public (IEnumerable<EmbedReference>, IEnumerable<EmbedReference>) PropertyIsValid(IEnumerable<EmbedReference> refs) {
            return refs.Fork(r => r.EmbedEntry.Fields.ContainsKey(r.Property) || _validFields.Contains(r.Property));
        }

        public FilterResults FilterRefs(IEnumerable<EmbedReference> refs) {
            // refs will either exist already or not
            var (Known, Unknown) = FactExists(refs);
            var (Global, Local) = FactIsGlobal(Known);
            // some known refs will be locked to the user
            var (Locked, Unlocked) = FactIsLocked(Local);
            var (Slugs, Properties) = FactOrProperty(Unlocked);
            var (Found, Missing) = PropertyIsValid(Properties);

            return new FilterResults(
                Known, Unknown, Global, Local, Locked, Unlocked, Slugs, Properties, Missing, Found);
        }

    }

}