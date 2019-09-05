using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Discord;

using Disunity.Disinfo.Interfaces;


namespace Disunity.Disinfo.Models.Entities {

    public class EmbedEntry : ITable {

        public string Id { get; set; }

        public string Guild { get; set; }

        public string Slug { get; set; }
        public string Title { get; set; }

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

        public Embed AsEmbed() {
            var builder = new EmbedBuilder().WithTitle(Title ?? Slug)
                                            .WithDescription(Description)
                                            .WithAuthor(Author)
                                            .WithUrl(Url)
                                            .WithImageUrl(ImageUrl)
                                            .WithThumbnailUrl(ThumbnailUrl)
                                            .WithColor(DiscordColor);

            var fields = new List<EmbedFieldBuilder>();

            if (Fields != null && Fields.Count > 0) {
                fields = fields.Concat(Fields.Keys
                                             .Where(k => Fields[k] != null)
                                             .Where(k => Fields[k] != "")
                                             .Select(k => new EmbedFieldBuilder().WithName(k).WithValue(Fields[k])))
                               .ToList();
            }

            if (Guild == "0") {
                var footer = new EmbedFooterBuilder()
                             .WithText("*Global*")
                             .WithIconUrl("https://image.flaticon.com/icons/png/512/65/65318.png");

                builder = builder.WithFooter(footer);
            } else if (Locked) {
                var footer = new EmbedFooterBuilder()
                             .WithText("*Locked*")
                             .WithIconUrl("https://cdn0.iconfinder.com/data/icons/mono2/100/lock-512.png");

                builder = builder.WithFooter(footer);
            }

            builder = builder.WithFields(fields);
            return builder.Build();
        }

        public bool IsEmpty =>
            string.IsNullOrEmpty(Title) &&
            string.IsNullOrEmpty(Description) &&
            string.IsNullOrEmpty(Author) &&
            string.IsNullOrEmpty(Url) &&
            string.IsNullOrEmpty(ImageUrl) &&
            string.IsNullOrEmpty(ThumbnailUrl) &&
            (Fields == null || Fields.Count == 0);

    }

}