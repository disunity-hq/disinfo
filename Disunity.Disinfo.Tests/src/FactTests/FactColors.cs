using Xunit;


namespace Disunity.Disinfo.Tests.FactTests {

    public class FactColors {

        [Fact]
        public void CorrectDiscordColorReturned() {
            var fact = new Fact() {
                Color = "Red"
            };    

            Assert.Equal(Discord.Color.Red, fact.DiscordColor);
        }

        [Fact]
        public void DefaultDiscordColorReturned() {
            var fact = new Fact();
            
            Assert.Equal(Discord.Color.Default, fact.DiscordColor);
        }

        [Fact]
        public void InvalidColorReturnsDefault() {
            var fact = new Fact() {
                Color = "foobar"
            };

            Assert.Equal(Discord.Color.Default, fact.DiscordColor);
        }

        [Fact]
        public void ColorsAreCaseInsensitive() {
            var fact = new Fact() {
                Color = "grEEn"
            };
            
            Assert.Equal(Discord.Color.Green, fact.DiscordColor);
        }

    }

}