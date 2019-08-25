using System.Threading.Tasks;

using Disunity.Disinfo.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Disunity.Disinfo {

    class Program {

        public static Task Main(string[] args) => RunAsync(args);

        public static async Task RunAsync(string[] args) {
            var configuration = new ConfigurationBuilder()
                                .AddEnvironmentVariables()
                                .Build();

            var services = new ServiceCollection();
            new Startup.Startup(configuration).ConfigureServices(services);

            var provider = services.BuildServiceProvider(); // Build the service provider
            provider.GetRequiredService<LoggingService>(); // Start the logging service
            provider.GetRequiredService<DispatchService>(); // Start the command handler service

            await provider.GetRequiredService<StartupService>().StartAsync(); // Start the startup service
            await Task.Delay(-1); // Keep the program alive
        }

    }

}