using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Services;
using Microsoft.Extensions.Logging;

namespace Lucy.App.LanguageServer;

[Service(Lifetime.Singleton)]
public class LanguageServerApp
{
    private readonly JsonRpcServer _jsonRpcServer;

    public LanguageServerApp(JsonRpcServer jsonRpcServer)
    {
        _jsonRpcServer = jsonRpcServer;
    }

    public static async Task<int> Main()
    {
        return await CreateServiceCollection()
            .BuildServiceProvider()
            .GetRequiredService<LanguageServerApp>()
            .Run();
    }

    private static IServiceCollection CreateServiceCollection()
    {
        return new ServiceCollection()
            .AddServicesFromCurrentAssembly()
            .AddJsonRpcServer(b =>
            {
                b.AddControllerFromCurrentAssembly();
                b.AddJsonConverter<SystemPathConverter>();

                if (DebugVsCodeRunner.IsVsCodeStartupRequested)
                    b.ListenOnNetworkEndpoint(DebugVsCodeRunner.NetworkEndpoint);
            })
            .AddLogging(x =>
            {
                x.AddSimpleConsole();
            });
    }

    private async Task<int> Run()
    {
        try
        {
            await _jsonRpcServer.Start();

            if (DebugVsCodeRunner.IsVsCodeStartupRequested)
                DebugVsCodeRunner.Launch();

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