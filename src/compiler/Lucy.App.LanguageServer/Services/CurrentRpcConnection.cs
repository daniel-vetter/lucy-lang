using System;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Common.ServiceDiscovery;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service]
    public class CurrentRpcConnection
    {
        private readonly IServiceProvider _serviceProvider;
        private JsonRpcServer? _connection;

        public CurrentRpcConnection(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Run()
        {
            var config = new JsonRpcConfig()
                .AddControllersFromCurrentAssembly()
                .TraceTo(GetTraceTarget())
                .SetControllerFactory(ControllerFactory);

            _connection = new JsonRpcServer();
            await _connection.Start(config);
            await _connection.WaitTillStopped();
            _connection = null;
        }

        private object ControllerFactory(Type type)
        {
            return _serviceProvider.GetService(type) ?? throw new Exception("Service provider returned null for type: " + type.Name);
        }

        public async Task SendNotification(string name, object? parameter)
        {
            if (_connection == null)
                throw new Exception("Can not send a notification to the language client because it is not connected");

            await _connection.SendNotification(name, parameter);
        }

        public async Task<T> SendRequest<T>(string name, object? parameter)
        {
            if (_connection == null)
                throw new Exception("Can not send a request to the language client because it is not connected");

            return await _connection.SendRequest<T>(name, parameter);
        }

        private IJsonRpcMessageTrace? GetTraceTarget()
        {
            var targetDir = Environment.GetEnvironmentVariable("LUCY_LANGUAGE_SERVER_RPC_TRACE_FILE");
            if (string.IsNullOrWhiteSpace(targetDir))
                return null;
            return new DiskTraceTarget(targetDir);
        }

        internal async Task Shutdown()
        {
            if (_connection == null)
                throw new Exception("Can close the connection because it was not opened");

            await _connection.Stop();
        }
    }
}
