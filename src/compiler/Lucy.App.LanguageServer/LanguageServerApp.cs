﻿using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Services;
using Lucy.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Lucy.App.LanguageServer;

[Service(Lifetime.Singleton)]
public class LanguageServerApp
{
    private readonly JsonRpcServer _jsonRpcServer;
    private readonly ILogger<LanguageServerApp> _logger;

    public LanguageServerApp(JsonRpcServer jsonRpcServer, ILogger<LanguageServerApp> logger)
    {
        _jsonRpcServer = jsonRpcServer;
        _logger = logger;
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

        Profiler.Attach();

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
                x.AddConsole(y => y.FormatterName = "Custom");
                x.AddConsoleFormatter<CustomConsoleFormatter, ConsoleFormatterOptions>();
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
            _logger.LogCritical(e, "A unexpected error occurred.");
            return -1;
        }
    }
}