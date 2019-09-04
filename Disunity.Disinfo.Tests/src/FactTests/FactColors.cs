using Disunity.Disinfo.Models;
using Disunity.Disinfo.Models.Entities;

using Xunit;


namespace Disunity.Disinfo.Tests.FactTests {

    public class FactColors {

        [Fact]
        public void CorrectDiscordColorReturned() {
            var fact = new EmbedEntry() {
                Color = "Red"
            };    

            Assert.Equal(Discord.Color.Red, fact.DiscordColor);
        }

        [Fact]
        public void DefaultDiscordColorReturned() {
            var fact = new EmbedEntry();
            
            Assert.Equal(Discord.Color.Default, fact.DiscordColor);
        }

        [Fact]
        public void InvalidColorReturnsDefault() {
            var fact = new EmbedEntry() {
                Color = "foobar"
            };

            Assert.Equal(Discord.Color.Default, fact.DiscordColor);
        }

        [Fact]
        public void ColorsAreCaseInsensitive() {
            var fact = new EmbedEntry() {
                Color = "grEEn"
            };
            
            Assert.Equal(Discord.Color.Green, fact.DiscordColor);
        }

    }

}