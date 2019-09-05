using System.Collections.Generic;

using BindingAttributes;

using Disunity.Disinfo.Extensions;
using Disunity.Disinfo.Models;
using Disunity.Disinfo.Models.Entities;

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

        /// <summary>
        /// Returns an EmbedEntry from the database or null
        /// </summary>
        /// <param name="query">A string which will be slugified to look up the entry.</param>
        /// <param name="guild">A string containing the Guild ID</param>
        /// <returns></returns>
        public EmbedEntry Query(string query, string guild = "0") {
            var slug = _slugHelper.GenerateSlug(query);
            EmbedEntry embedEntry = null;
            _dbService.WithTable<EmbedEntry>(t => { embedEntry = t.QueryEntry(slug, guild); });
            return embedEntry;
        }

        public void Delete(EmbedEntry entry) {
            _dbService.WithTable<EmbedEntry>(t => { t.Delete(entry.Id); });
        }

        public EmbedEntry Save(EmbedEntry embedEntry) {
            embedEntry.Slug = _slugHelper.GenerateSlug(embedEntry.Slug);
            embedEntry.AsEmbed();

            var oldFact = Query(embedEntry.Slug, embedEntry.Guild);

            if (oldFact != null) {
                _dbService.WithTable<EmbedEntry>(t => t.UpdateEntry(embedEntry));
            } else {
                _dbService.WithTable<EmbedEntry>(t => t.InsertEntry(embedEntry));
            }

            return embedEntry;
        }

        public EmbedEntry Update(string query, string property, string value, string guild = "0") {
            var slug = _slugHelper.GenerateSlug(query);

            if (value != null && value.ToLower() == "null") {
                value = null;
            }

            var entry = Query(slug, guild);

            if (entry != null & property == null && value == null) {
                Delete(entry);
                return null;
            }

            if (entry == null) {
                entry = new EmbedEntry {Slug = slug, Guild = guild};
            }

            if (entry.Fields == null) {
                entry.Fields = new Dictionary<string, string>();
            }

            if (property == null) {
                entry.Description = value;
                entry = Save(entry);
            } else {

                switch (property.ToLower()) {
                    case "description":
                        entry.Description = value;
                        break;

                    case "author":
                        entry.Author = value;
                        break;

                    case "color":
                        entry.Color = value;
                        break;

                    case "url":
                        entry.Url = value;
                        break;

                    case "thumbnail":
                        entry.ThumbnailUrl = value;
                        break;

                    case "image":
                        entry.ImageUrl = value;
                        break;

                    case "locked":
                        entry.Locked = value?.ToLower() == "true";
                        break;

                    default: {
                        if (value != null) {
                            entry.Fields[property] = value;
                        } else if (entry.Fields.ContainsKey(property)) {
                            entry.Fields.Remove(property);
                        }

                        break;
                    }
                }
            }
            
            if (entry.IsEmpty) {
                Delete(entry);
                return null;
            }

            return Save(entry);
        }

        public void UpdateReference(EmbedReference embedRef, string value, string guild = "0") {
            embedRef.EmbedEntry = Update(embedRef.Slug, embedRef.Property, value, guild);
        } 

    }

}