using System;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Common.ServiceDiscovery;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service(Lifetime.Singleton)]
    public class CurrentJsonRpcConnection
    {
        private readonly IServiceProvider _serviceProvider;
        private JsonRpcServer? _connection;

        public CurrentJsonRpcConnection(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

        private IJsonRpcMessageTraceTarget? GetTraceTarget()
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
