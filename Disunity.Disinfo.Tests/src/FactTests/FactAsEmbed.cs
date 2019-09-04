using System.Collections.Generic;
using System.Linq;

using Discord;

using Disunity.Disinfo.Models.Entities;

using Xunit;


namespace Disunity.Disinfo.Tests.FactTests {

    public class FactAsEmbed {

        private EmbedEntry _embedEntry;
        private Discord.Embed _discordEmbed;

        public FactAsEmbed() {
            _embedEntry = new EmbedEntry() {
                Id = "id",
                Description = "description",
                Author = "author",
                Url = "http://foo.com/",
                ImageUrl = "http://foo.com/foo.png",
                ThumbnailUrl = "http://foo.com/foo.png",
                Color = "red",
                Fields = new Dictionary<string, string>() {
                    {"foo", "bar"},
                    {"biz", "baz"}
                },
                Locked = true
            };

            _discordEmbed = _embedEntry.AsEmbed();
        }

        [Fact]
        public void EmbedPropertiesCorrect() {
            Assert.Equal(_discordEmbed.Title, _embedEntry.Id);
            Assert.Equal(_discordEmbed.Description, _embedEntry.Description);
            Assert.Equal(_discordEmbed.Author?.Name, _embedEntry.Author);
            Assert.Equal(_discordEmbed.Url, _embedEntry.Url);
            Assert.Equal(_discordEmbed.Image?.Url, _embedEntry.ImageUrl);
            Assert.Equal(_discordEmbed.Thumbnail?.Url, _embedEntry.ThumbnailUrl);
            Assert.Equal(_discordEmbed.Color, _embedEntry.DiscordColor);
            Assert.Equal("*Locked*", _discordEmbed.Footer?.Text);
            Assert.True(_discordEmbed.Fields.Any(f => f.Name == "foo" && f.Value == "bar"));
            Assert.True(_discordEmbed.Fields.Any(f => f.Name == "biz" && f.Value == "baz"));
        } 

    }

}