﻿using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer.Services;
using Lucy.Infrastructure.RpcServer;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer
{
    [Service(Lifetime.Singleton)]
    public class LanguageServerApp
    {
        private readonly JsonRpcServer _jsonRpcServer;

        public LanguageServerApp(JsonRpcServer jsonRpcServer)
        {
            _jsonRpcServer = jsonRpcServer;
        }

        public static async Task<int> Main() => await CreateServiceProvider().GetRequiredService<LanguageServerApp>().Run();

        public static ServiceProvider CreateServiceProvider()
        {
            return new ServiceCollection()
                .AddServicesFromCurrentAssembly()
                .AddJsonRpcServer(b =>
                {
                    b.AddControllerFromCurrentAssembly();
                    b.AddJsonConverter<SystemPathConverter>();
                })
                .BuildServiceProvider();
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
}
