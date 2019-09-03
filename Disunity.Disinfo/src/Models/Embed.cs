using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Discord;

using Disunity.Disinfo.Interfaces;


namespace Disunity.Disinfo.Models {

    public class Embed : ITable {

        public string Id { get; set; }
        public string Description { get; set; }

        public string Author { get; set; }

        public string Color { get; set; }

        public string Url { get; set; }

        public string ImageUrl { get; set; }

        public string ThumbnailUrl { get; set; }

        public bool Locked { get; set; }

        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();

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

        public Discord.Embed AsEmbed() {
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

            if (Locked) {
                var footer = new EmbedFooterBuilder()
                             .WithText("*Locked*")
                             .WithIconUrl("https://cdn0.iconfinder.com/data/icons/mono2/100/lock-512.png");

                builder.WithFooter(footer);
            }

            return builder.Build();
        }

        public bool IsEmpty =>
            string.IsNullOrEmpty(Description) &&
            string.IsNullOrEmpty(Author) &&
            string.IsNullOrEmpty(Url) &&
            string.IsNullOrEmpty(ImageUrl) &&
            string.IsNullOrEmpty(ThumbnailUrl) &&
            (Fields == null || Fields.Count == 0);

    }

}