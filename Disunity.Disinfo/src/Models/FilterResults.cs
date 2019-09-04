using Refs = System.Collections.Generic.IEnumerable<Disunity.Disinfo.Models.EmbedRef>;


namespace Disunity.Disinfo.Models {

    public class FilterResults {

        public Refs Known { get; }
        public Refs Unknown { get; }
        public Refs Global { get; }
        public Refs Local { get; }
        public Refs Locked { get; }
        public Refs Unlocked { get; }

        public Refs Slugs { get; }
        public Refs Properties { get; }

        public FilterResults(Refs known, Refs unknown,
                             Refs global, Refs local,
                             Refs locked, Refs unlocked,
                             Refs slugs, Refs properties) {
            Known = known;
            Unknown = unknown;
            Global = global;
            Local = local;
            Locked = locked;
            Unlocked = unlocked;
            Slugs = slugs;
            Properties = properties;
        }

    }

}