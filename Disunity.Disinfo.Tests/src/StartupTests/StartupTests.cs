using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

using Xunit;


namespace Disunity.Disinfo.Tests.StartupTests {

    public class StartupTests {

//        [Fact]
//        public void ConfigurationSet() {
//            var config = new Mock<IConfigurationRoot>();
//            var startup = new Startup.Startup(config.Object);
//            Assert.Equal(startup.Configuration, config.Object);
//        }
//
//
//        [Fact]
//        public void ServicesConfigured() {
//            var config = new Mock<IConfigurationRoot>();
//            var services = new Mock<IServiceCollection>();
//            services.Setup(m => m.AddSingleton(config.Object)).Returns(services.Object);
//            services.Setup(m => m.AddSingleton<SlugHelperConfig>()).Returns(services.Object);
//
//            var startup = new Startup.Startup(config.Object);
//            startup.ConfigureServices(services.Object);
//            
//            services.Verify(m => m.AddSingleton(config), Times.Once);
//            services.Verify(m => m.AddSingleton<SlugHelperConfig>(), Times.Once);
//        }
    }

}