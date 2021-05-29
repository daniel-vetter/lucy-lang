using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var sp = new ServiceCollection()
                .AddServicesFromCurrentAssembly()
                .BuildServiceProvider();

            return await sp.GetRequiredService<App>().Run();
        }
    }

    [Service(ServiceLifetime.Singleton)]
    internal class App
    {
        private readonly CurrentRpcConnection _currentRpcConnection;

        public App(CurrentRpcConnection currentRpcConnection)
        {
            _currentRpcConnection = currentRpcConnection;
        }

        internal async Task<int> Run()
        {
            try
            {
                await _currentRpcConnection.Run();
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return -1;
            }
        }
    }
}
