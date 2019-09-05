using System.Collections.Generic;

using Disunity.Disinfo.Models.Entities;


namespace Disunity.Disinfo.Extensions {

    public static class DictionaryExtensions {

        public static EmbedEntry AsEntry(this Dictionary<string, string> data, string slug, string guild = "0") {
            var entry = new EmbedEntry {
                Slug = slug,
                Guild = guild
            };

            foreach (var (property, value) in data) {
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

                    default:
                        if (value != null) {
                            entry.Fields[property] = value;
                        } else if (entry.Fields.ContainsKey(property)) {
                            entry.Fields.Remove(property);
                        }

                        break;

                }
            }

            return entry;
        }

    }

}