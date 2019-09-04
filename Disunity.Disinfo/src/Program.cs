using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using BindingAttributes;

using Discord.Commands;
using Discord.WebSocket;

using Disunity.Disinfo.Interfaces;
using Disunity.Disinfo.Options;
using Disunity.Disinfo.Services;
using Disunity.Disinfo.Services.Scoped;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SharpYaml.Serialization;

using Slugify;


namespace Disunity.Disinfo {

    class Program {

        public static Task Main(string[] args) => RunAsync(args);

        private static IConfigurationRoot BuildConfiguration() {
            Console.WriteLine("Building configuration...");

            return new ConfigurationBuilder()
                   .AddEnvironmentVariables()
                   .Build();
        }

        private static IServiceProvider BuildServiceProvider(IConfigurationRoot configuration) {
            Console.WriteLine("Building service provider...");
            var services = new ServiceCollection();
            BindServices(services, configuration);
            return services.BuildServiceProvider();
        }

        private static void BindServices(IServiceCollection services, IConfigurationRoot configuration) {
            Console.WriteLine("Binding services...");

            OptionsAttribute.ConfigureOptions(services, configuration);
            BindingAttribute.ConfigureBindings(services);
            FactoryAttribute.ConfigureFactories(services);

            var serviceNames = services.Where(d => d.ServiceType == typeof(ContextService));

            foreach (var desc in serviceNames) {
                Console.WriteLine($"{desc.ServiceType} => {desc.ImplementationType ?? desc.ImplementationFactory ?? desc.ImplementationInstance} as {desc.Lifetime}");
            }

            services // bind third-party services (can't add binding attributes to classes we don't control)
                .AddLogging(builder => builder.AddConsole())
                .AddSingleton(configuration)
                .AddSingleton<Serializer>(new Serializer())
                .AddSingleton<ISlugHelper, SlugHelper>()
                .AddSingleton<CommandService>();
        }

        private static async Task Boot(IServiceProvider provider) {
            BootAllOptions(provider);
            var bootService = provider.GetRequiredService<IBootService>();
            bootService.Boot();
        }

        public static void HandleOptionsValidationException(OptionsValidationException e) {
            Console.WriteLine("A configuration error has occured:");

            foreach (var error in e.Failures) {
                Console.WriteLine(error);
            }
        }

        private static void BootAllOptions(IServiceProvider provider) {
            foreach(var type in Assembly.GetCallingAssembly().GetTypes()) {
                var attr = type.GetCustomAttribute<OptionsAttribute>();

                if (attr != null) {
                    Console.WriteLine($"Loading options: {type}");
                    var optionsInterface = typeof(IOptions<>);
                    var optionsType = optionsInterface.MakeGenericType(type);
                    provider.GetRequiredService(optionsType);
                }
            }
        }

        public static async Task RunAsync(string[] args) {
            Console.WriteLine("Disinfo starting up...");

            try {
                var configuration = BuildConfiguration();
                var provider = BuildServiceProvider(configuration);
                await Boot(provider);
                await Task.Delay(-1); // Keep the program alive
            }
            catch (OptionsValidationException e) {
                HandleOptionsValidationException(e);
            }
            catch (TargetInvocationException e) {
                if (e.InnerException != null) {
                    if (e.InnerException is OptionsValidationException optionsError) {
                        HandleOptionsValidationException(optionsError);
                    }
                }
            }
        }

    }

}