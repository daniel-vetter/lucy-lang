using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Microsoft.Extensions.Logging;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure
{
    [Service(Lifetime.Singleton)]
    public class TransportProvider
    {
        private readonly ILogger<TransportProvider> _logger;
        private readonly Task<Stream>? _networkStream;

        public TransportProvider(JsonRpcConfig config, ILogger<TransportProvider> logger)
        {
            _logger = logger;
            if (config.NetworkEndpoint != null)
                _networkStream = CreateServer(config.NetworkEndpoint);
        }

        private async Task<Stream> CreateServer(IPEndPoint endpoint)
        {
            using Socket listener = new(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endpoint);
            listener.Listen();
            _logger.LogInformation("Waiting for incoming connection");
            var connection = await listener.AcceptAsync();

            _logger.LogInformation("Incoming connection received. Closing listener. No further client connections will be accepted.");
            listener.Close();
            return new NetworkStream(connection);
        }

        public async Task<Stream> GetOutputStream()
        {
            if (_networkStream != null)
                return await _networkStream;
            
            return Console.OpenStandardOutput();
        }

        public async Task<Stream> GetInputStream()
        {
            if (_networkStream != null)
                return await _networkStream;
            
            return Console.OpenStandardInput();
        }
    }
}
