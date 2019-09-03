using System.Collections.Generic;
using System.Linq;

using Discord;

using Xunit;

using Embed = Disunity.Disinfo.Models.Embed;


namespace Disunity.Disinfo.Tests.FactTests {

    public class FactAsEmbed {

        private Embed _embed;
        private Discord.Embed _discordEmbed;

        public FactAsEmbed() {
            _embed = new Embed() {
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

            _discordEmbed = _embed.AsEmbed();
        }

        [Fact]
        public void EmbedPropertiesCorrect() {
            Assert.Equal(_discordEmbed.Title, _embed.Id);
            Assert.Equal(_discordEmbed.Description, _embed.Description);
            Assert.Equal(_discordEmbed.Author?.Name, _embed.Author);
            Assert.Equal(_discordEmbed.Url, _embed.Url);
            Assert.Equal(_discordEmbed.Image?.Url, _embed.ImageUrl);
            Assert.Equal(_discordEmbed.Thumbnail?.Url, _embed.ThumbnailUrl);
            Assert.Equal(_discordEmbed.Color, _embed.DiscordColor);
            Assert.Equal("*Locked*", _discordEmbed.Footer?.Text);
            Assert.True(_discordEmbed.Fields.Any(f => f.Name == "foo" && f.Value == "bar"));
            Assert.True(_discordEmbed.Fields.Any(f => f.Name == "biz" && f.Value == "baz"));
        } 

    }

}