using System.Linq;

using Refs = System.Collections.Generic.IEnumerable<Disunity.Disinfo.Models.EmbedReference>;


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
        
        public Refs Missing { get; }
        public Refs Found { get; }


        public FilterResults(Refs known, Refs unknown,
                             Refs global, Refs local,
                             Refs locked, Refs unlocked,
                             Refs slugs, Refs properties, Refs missing, Refs found) {
            Known = known;
            Unknown = unknown;
            Global = global;
            Local = local;
            Locked = locked;
            Unlocked = unlocked;
            Slugs = slugs;
            Properties = properties;
            Missing = missing;
            Found = found;
        }

        public Refs Valid => Slugs.Concat(Found);
        public Refs Invalid => Unknown.Concat(Missing);

        public override string ToString() {
            var result = "";

            result += "Known:\n";

            foreach (var r in Known) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Unknown:\n";

            foreach (var r in Unknown) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Global:\n";

            foreach (var r in Global) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Local:\n";

            foreach (var r in Local) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Locked:\n";

            foreach (var r in Locked) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Unlocked:\n";

            foreach (var r in Unlocked) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Slugs:\n";

            foreach (var r in Slugs) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Properties:\n";

            foreach (var r in Properties) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }

            result += "Missing:\n";

            foreach (var r in Missing) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Found:\n";

            foreach (var r in Found) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }

            result += "Valid:\n";

            foreach (var r in Valid) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }
            result += "Invalid:\n";

            foreach (var r in Invalid) {
                result += $"-- {r.Slug}.{r.Property} Entry is null: {r.EmbedEntry == null}\n";
            }


            return result;
        }

    }

}