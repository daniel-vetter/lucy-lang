using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer.Services;

namespace Lucy.Feature.LanguageServer
{
    [Service]
    public class LanguageServerFacade
    {
        private readonly CurrentRpcConnection _languageServer;

        public LanguageServerFacade(CurrentRpcConnection languageServer)
        {
            _languageServer = languageServer;
        }

        public async Task Run()
        {
            await _languageServer.Run();
        }
    }
}
