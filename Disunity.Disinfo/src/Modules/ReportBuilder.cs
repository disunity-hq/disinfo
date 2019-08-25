using System.Collections.Generic;
using System.Linq;

using Discord;

using FactRefs = System.Collections.Generic.IEnumerable<Disunity.Disinfo.Modules.FactRef>;


namespace Disunity.Disinfo.Modules {

    public class ReportBuilder {

        private FactRefs _createdRefs;
        private FactRefs _deletedRefs;
        private FactRefs _updatedRefs;
        private FactRefs _missingRefs;
        private FactRefs _lockedRefs;
        private FactRefs _skippedRefs;

        private EmbedFieldBuilder ListField(string name, IEnumerable<string> items) {
            var joined = string.Join(", ", items);
            return new EmbedFieldBuilder().WithName(name).WithValue(joined);
        }

        public ReportBuilder WithCreatedRefs(FactRefs refs) {
            _createdRefs = _createdRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithDeletedRefs(FactRefs refs) {
            _deletedRefs = _deletedRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithUpdatedRefs(FactRefs refs) {
            _updatedRefs = _updatedRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithMissingRefs(FactRefs refs) {
            _missingRefs = _missingRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithLockedRefs(FactRefs refs) {
            _lockedRefs = _lockedRefs?.Concat(refs) ?? refs;
            return this;
        }

        public ReportBuilder WithSkippedRefs(FactRefs refs) {
            _skippedRefs = _skippedRefs?.Concat(refs) ?? refs;
            return this;
        }

        private FactRef SingleUpdate() {
            if (_updatedRefs == null || _updatedRefs.Count() > 1) {
                return null;
            }

            if ((_createdRefs?.Any() ?? false) ||
                (_deletedRefs?.Any() ?? false) ||
                (_missingRefs?.Any() ?? false) ||
                (_skippedRefs?.Any() ?? false) ||
                (_lockedRefs?.Any() ?? false)) {
                return null;
            }

            return _updatedRefs.First();
        }

        private FactRef SingleCreated() {
            if (_createdRefs == null || _createdRefs.Count() > 1) {
                return null;
            }

            if ((_updatedRefs?.Any() ?? false) ||
                (_deletedRefs?.Any() ?? false) ||
                (_missingRefs?.Any() ?? false) ||
                (_skippedRefs?.Any() ?? false) ||
                (_lockedRefs?.Any() ?? false)) {
                return null;
            }

            return _createdRefs.First();
        }

        private FactRef SingleRef() {
            return SingleCreated() ?? SingleUpdate();
        }

        private Embed SingleEmbed() {
            return SingleRef()?.Fact?.AsEmbed();
        }

        private Embed ReportEmbed() {
            var fields = new List<EmbedFieldBuilder>();

            if (_createdRefs?.Any() ?? false) {
                fields.Add(ListField("I created", _createdRefs.Select(r => r.Fact.Id)));
            }

            if (_deletedRefs?.Any() ?? false) {
                fields.Add(ListField("I deleted", _deletedRefs.Select(r => r.Fact.Id)));
            }

            if (_updatedRefs?.Any() ?? false) {
                fields.Add(ListField("I updated", _updatedRefs.Select(r => r.Fact.Id)));
            }

            if (_lockedRefs?.Any() ?? false) {
                fields.Add(ListField("These are locked", _lockedRefs.Select(r => r.Fact.Id)));
            }

            if (_missingRefs?.Any() ?? false) {
                fields.Add(ListField("These didn't exist", _missingRefs.Select(r => r.InputStr)));
            }

            if (_skippedRefs?.Any() ?? false) {
                fields.Add(ListField("I skipped these", _skippedRefs.Select(r => r.InputStr)));
            }

            return new EmbedBuilder().WithFields(fields).Build();

        }

        public Embed Build() {
            return SingleEmbed() ?? ReportEmbed();
        }

    }

}