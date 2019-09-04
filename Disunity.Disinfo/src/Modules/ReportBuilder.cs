using System.Collections.Generic;
using System.Linq;

using Discord;

using Disunity.Disinfo.Models;

using FactRefs = System.Collections.Generic.IEnumerable<Disunity.Disinfo.Models.EmbedRef>;


namespace Disunity.Disinfo.Modules {

    public class ReportBuilder {

        public FactRefs CreatedRefs { get; private set; }
        public FactRefs DeletedRefs { get; private set; }
        public FactRefs UpdatedRefs { get; private set; }
        public FactRefs MissingRefs { get; private set; }
        public FactRefs LockedRefs { get; private set; }
        public FactRefs SkippedRefs { get; private set; }
        public FactRefs GlobalRefs { get; private set; }

        private EmbedFieldBuilder ListField(string name, IEnumerable<string> items) {
            var joined = string.Join(", ", items);
            return new EmbedFieldBuilder().WithName(name).WithValue(joined);
        }

        public ReportBuilder WithCreatedRefs(FactRefs refs) {
            CreatedRefs = CreatedRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithDeletedRefs(FactRefs refs) {
            DeletedRefs = DeletedRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithUpdatedRefs(FactRefs refs) {
            UpdatedRefs = UpdatedRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithMissingRefs(FactRefs refs) {
            MissingRefs = MissingRefs?.Concat(refs) ?? refs;
            return this;
        }
        
        public ReportBuilder WithGlobalRefs(FactRefs refs) {
            GlobalRefs = GlobalRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithLockedRefs(FactRefs refs) {
            LockedRefs = LockedRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithSkippedRefs(FactRefs refs) {
            SkippedRefs = SkippedRefs?.Concat(refs) ?? refs;
            return this;
        }

        private EmbedRef SingleUpdate() {
            if (UpdatedRefs == null || UpdatedRefs.Count() > 1) {
                return null;
            }

            if ((CreatedRefs?.Any() ?? false) ||
                (DeletedRefs?.Any() ?? false) ||
                (MissingRefs?.Any() ?? false) ||
                (SkippedRefs?.Any() ?? false) ||
                (GlobalRefs?.Any() ?? false) ||
                (LockedRefs?.Any() ?? false)) {
                return null;
            }

            return UpdatedRefs.First();
        }

        private EmbedRef SingleCreated() {
            if (CreatedRefs == null || CreatedRefs.Count() > 1) {
                return null;
            }

            if ((UpdatedRefs?.Any() ?? false) ||
                (DeletedRefs?.Any() ?? false) ||
                (MissingRefs?.Any() ?? false) ||
                (SkippedRefs?.Any() ?? false) ||
                (GlobalRefs?.Any() ?? false) ||
                (LockedRefs?.Any() ?? false)) {
                return null;
            }

            return CreatedRefs.First();
        }

        private EmbedRef SingleRef() {
            return SingleCreated() ?? SingleUpdate();
        }

        private Embed SingleEmbed() {
            return SingleRef()?.EmbedEntry?.AsEmbed();
        }

        private Embed ReportEmbed() {
            var fields = new List<EmbedFieldBuilder>();

            if (CreatedRefs?.Any() ?? false) {
                fields.Add(ListField("I created", CreatedRefs.Select(r => r.EmbedEntry.Slug)));
            }

            if (DeletedRefs?.Any() ?? false) {
                fields.Add(ListField("I deleted", DeletedRefs.Select(r => r.EmbedEntry.Slug)));
            }

            if (UpdatedRefs?.Any() ?? false) {
                fields.Add(ListField("I updated", UpdatedRefs.Select(r => r.EmbedEntry.Slug)));
            }

            if (GlobalRefs?.Any() ?? false) {
                fields.Add(ListField("These are global", GlobalRefs.Select(r => r.EmbedEntry.Slug)));
            }

            if (LockedRefs?.Any() ?? false) {
                fields.Add(ListField("These are locked", LockedRefs.Select(r => r.EmbedEntry.Slug)));
            }

            if (MissingRefs?.Any() ?? false) {
                fields.Add(ListField("These didn't exist", MissingRefs.Select(r => r.Input)));
            }

            if (SkippedRefs?.Any() ?? false) {
                fields.Add(ListField("I skipped these", SkippedRefs.Select(r => r.Input)));
            }

            return new EmbedBuilder().WithFields(fields).Build();

        }

        public Embed Build() {
            return SingleEmbed() ?? ReportEmbed();
        }


    }

}