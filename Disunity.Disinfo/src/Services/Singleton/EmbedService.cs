using System;
using System.Collections.Generic;

using BindingAttributes;

using Disunity.Disinfo.Interfaces;
using Disunity.Disinfo.Models.Entities;
using Disunity.Disinfo.Extensions;

using Slugify;


namespace Disunity.Disinfo.Services.Singleton {

    [AsSingleton]
    public class EmbedService {

        private readonly DbService _dbService;
        private readonly ISlugHelper _slugHelper;

        public EmbedService(DbService dbService, ISlugHelper slugHelper) {
            _dbService = dbService;
            _slugHelper = slugHelper;
        }

        public EmbedEntry Query(string input, string guild = "0") {
            var slug = _slugHelper.GenerateSlug(input);
            EmbedEntry embedEntry = null;
            _dbService.WithTable<EmbedEntry>(t => { embedEntry = t.FindById(slug, guild); });
            return embedEntry;
        }

        public bool Forget(string input, string guild = "0") {
            var slug = _slugHelper.GenerateSlug(input);
            var fact = Query(slug, guild);

            if (fact == null) {
                return false;
            }

            _dbService.WithTable<EmbedEntry>(t => { t.Delete(o => o.Slug == slug && 
                                                                  o.Guild == guild); });
            return true;
        }

        public EmbedEntry Learn(EmbedEntry embedEntry) {
            embedEntry.Slug = _slugHelper.GenerateSlug(embedEntry.Slug);
            var oldFact = Query(embedEntry.Slug, embedEntry.Guild);

            if (oldFact != null) {
                _dbService.WithTable<EmbedEntry>(t => t.UpdateEntry(oldFact));
            } else {
                _dbService.WithTable<EmbedEntry>(t => t.InsertEntry(embedEntry));
            }

            return embedEntry;
        }

        public EmbedEntry Learn(string input, string description, string guild = "0") {
            var slug = _slugHelper.GenerateSlug(input);
            var fact = new EmbedEntry {Slug = slug, Guild = guild, Description = description};
            return Learn(fact);
        }

        public EmbedEntry Update(string input, string key, string value, string guild = "0") {
            var slug = _slugHelper.GenerateSlug(input);

            if (value != null && value.ToLower() == "null") {
                value = null;
            }

            var fact = Query(slug, guild);

            if (fact == null) {
                fact = new EmbedEntry() {Slug = slug, Guild = guild};
                _dbService.WithTable<EmbedEntry>(t => t.InsertEntry(fact));
            }

            if (fact.Fields == null) {
                fact.Fields = new Dictionary<string, string>();
            }

            var lowerKey = key?.ToLower() ?? "description";

            if (lowerKey == "description") {
                fact.Description = value;
            } else if (lowerKey == "author") {
                fact.Author = value;
            } else if (lowerKey == "color") {
                fact.Color = value;
            } else if (lowerKey == "url") {
                fact.Url = value;
            } else if (lowerKey == "thumbnail") {
                fact.ThumbnailUrl = value;
            } else if (lowerKey == "image") {
                fact.ImageUrl = value;
            } else if (lowerKey == "locked") {
                fact.Locked = value?.ToLower() == "true";
            }else if (value != null) {
                fact.Fields[lowerKey] = value;
            } else if (fact.Fields.ContainsKey(key)) {
                fact.Fields.Remove(key);
            }
            
            _dbService.WithTable<EmbedEntry>(t => t.UpdateEntry(fact));

            return fact;
        }

        public EmbedEntry Update(Dictionary<string, string> factData) {
            var slug = _slugHelper.GenerateSlug(factData["Slug"]);
            var guild = factData["Guild"];

            factData.Remove("Slug");
            factData.Remove("Guild");

            EmbedEntry embedEntry = null;

            foreach (var (key, value) in factData) {
                embedEntry = Update(slug, key, value, guild);
            }

            return embedEntry;
        }

    }

}