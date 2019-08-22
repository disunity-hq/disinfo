using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Discord;

using Disunity.Disinfo.Interfaces;


namespace Disunity.Disinfo {

    public class Fact : ITable {

        public string Id { get; set; }
        public string Description { get; set; }

        public string Author { get; set; }

        public string Color { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }

        public string ThumbnailUrl { get; set; }

        public Dictionary<string, string> Fields { get; set; }

        public Color DiscordColor {
            get {
                if (Color == null) {
                    return Discord.Color.Default;
                }

                var colors = typeof(Color).GetFields(BindingFlags.Static | BindingFlags.Public)
                                          .Where(fi => fi.FieldType == typeof(Color))
                                          .ToList();

                foreach (var color in colors) {
                    if (string.Equals(color.Name, Color, StringComparison.CurrentCultureIgnoreCase)) {
                        return (Color) color.GetValue(null);
                    }
                }

                return Discord.Color.Default;

            }
        }

        public Embed AsEmbed() {
            var builder = new EmbedBuilder().WithTitle(Id)
                                            .WithDescription(Description)
                                            .WithAuthor(Author)
                                            .WithUrl(Url)
                                            .WithImageUrl(ImageUrl)
                                            .WithThumbnailUrl(ThumbnailUrl)
                                            .WithColor(DiscordColor);

            if (Fields != null && Fields.Count > 0) {
                builder.Fields = Fields.Keys
                                       .Where(k => Fields[k] != null)
                                       .Where(k => Fields[k] != "")
                                       .Select(k => new EmbedFieldBuilder().WithName(k).WithValue(Fields[k]))
                                       .ToList();
            }

            return builder.Build();
        }

    }

}