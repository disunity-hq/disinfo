using System.Threading.Tasks;

using BindingAttributes;

using Disunity.Disinfo.Interfaces;


namespace Disunity.Disinfo.Services.Singleton {

    [AsSingleton(typeof(IBootService))]
    public class StartupService : IBootService {

        private readonly ClientService _discord;

        public StartupService(ClientService discord) {
            _discord = discord;
        }

        public async Task Boot() {
            await _discord.Start();
        }

    }

}