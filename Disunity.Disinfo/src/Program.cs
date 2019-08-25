using System;
using System.Threading.Tasks;

using Discord;

using Disunity.Disinfo.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;


namespace Disunity.Disinfo {

    class Program {

        public static Task Main(string[] args) => RunAsync(args);

        public static async Task RunAsync(string[] args) {
            Console.WriteLine("Disinfo starting up...");

            try {
                Console.WriteLine("-- Building configuration");
                var configuration = new ConfigurationBuilder()
                                    .AddEnvironmentVariables()
                                    .Build();

                var services = new ServiceCollection();
                new Startup.Startup(configuration).ConfigureServices(services);
                Console.WriteLine("-- Configuring services");
                var provider = services.BuildServiceProvider(); // Build the service provider
                var logger = provider.GetRequiredService<ILogger<Program>>(); // Start the logging service
                logger.LogInformation("-- Logging initialized");
                provider.GetRequiredService<DispatchService>(); // Start the command handler service
                logger.LogInformation("-- Dispatch service started");
                logger.LogInformation("Booting client...");
                await provider.GetRequiredService<StartupService>().StartAsync(); // Start the startup service
                await Task.Delay(-1); // Keep the program alive
            }
            catch (Exception e) {
                Console.WriteLine("A fatal exception has ocurred.");
                Console.WriteLine(e.Message);
            }
        }

    }

}