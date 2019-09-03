using Disunity.Disinfo.Models;

using Xunit;


namespace Disunity.Disinfo.Tests.FactTests {

    public class FactColors {

        [Fact]
        public void CorrectDiscordColorReturned() {
            var fact = new Embed() {
                Color = "Red"
            };    

            Assert.Equal(Discord.Color.Red, fact.DiscordColor);
        }

        [Fact]
        public void DefaultDiscordColorReturned() {
            var fact = new Embed();
            
            Assert.Equal(Discord.Color.Default, fact.DiscordColor);
        }

        [Fact]
        public void InvalidColorReturnsDefault() {
            var fact = new Embed() {
                Color = "foobar"
            };

            Assert.Equal(Discord.Color.Default, fact.DiscordColor);
        }

        [Fact]
        public void ColorsAreCaseInsensitive() {
            var fact = new Embed() {
                Color = "grEEn"
            };
            
            Assert.Equal(Discord.Color.Green, fact.DiscordColor);
        }

    }

}