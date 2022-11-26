using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Lucy.App.LanguageServer.Services;

namespace Lucy.App.LanguageServer;

[Service(Lifetime.Singleton)]
public class LanguageServerApp
{
    private readonly JsonRpcServer _jsonRpcServer;

    public LanguageServerApp(JsonRpcServer jsonRpcServer)
    {
        _jsonRpcServer = jsonRpcServer;
    }

    public static async Task<int> Main() => await CreateServiceCollection().BuildServiceProvider().GetRequiredService<LanguageServerApp>().Run();

    public static IServiceCollection CreateServiceCollection()
    {
        return new ServiceCollection()
            .AddServicesFromCurrentAssembly()
            .AddJsonRpcServer(b =>
            {
                b.AddControllerFromCurrentAssembly();
                b.AddJsonConverter<SystemPathConverter>();
            });
    }

    internal async Task<int> Run()
    {
        try
        {
            await _jsonRpcServer.Start();
            await _jsonRpcServer.WaitTillStopped();
            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e); //TODO: ILogger
            return -1;
        }
    }
}