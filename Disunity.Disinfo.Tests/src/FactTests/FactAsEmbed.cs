using System.Collections.Generic;
using System.Linq;

using Discord;

using Xunit;


namespace Disunity.Disinfo.Tests.FactTests {

    public class FactAsEmbed {

        private Fact _fact;
        private Embed _embed;

        public FactAsEmbed() {
            _fact = new Fact() {
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

            _embed = _fact.AsEmbed();
        }

        [Fact]
        public void EmbedPropertiesCorrect() {
            Assert.Equal(_embed.Title, _fact.Id);
            Assert.Equal(_embed.Description, _fact.Description);
            Assert.Equal(_embed.Author?.Name, _fact.Author);
            Assert.Equal(_embed.Url, _fact.Url);
            Assert.Equal(_embed.Image?.Url, _fact.ImageUrl);
            Assert.Equal(_embed.Thumbnail?.Url, _fact.ThumbnailUrl);
            Assert.Equal(_embed.Color, _fact.DiscordColor);
            Assert.Equal("*Locked*", _embed.Footer?.Text);
            Assert.True(_embed.Fields.Any(f => f.Name == "foo" && f.Value == "bar"));
            Assert.True(_embed.Fields.Any(f => f.Name == "biz" && f.Value == "baz"));
        } 

    }

}