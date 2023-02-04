using System.IO.Pipelines;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer.Internal;

[Service(Lifetime.Singleton)]
public class OutgoingMessageWriter
{
    private readonly Channel<Message> _outbox = Channel.CreateUnbounded<Message>();
    private readonly JsonRpcConfig _config;
    private readonly JsonRpcSerializer _serializer;
    private readonly TransportProvider _transportProvider;
    private readonly Worker _worker = new();
    private PipeWriter? _output;

    public OutgoingMessageWriter(JsonRpcConfig config, JsonRpcSerializer serializer, TransportProvider transportProvider)
    {
        _config = config;
        _serializer = serializer;
        _transportProvider = transportProvider;
    }

    public async ValueTask Write(Message message)
    {
        await _outbox.Writer.WriteAsync(message);
    }

    public void Start() => _worker.Start(_ => Run());
    public async Task Stop()
    {
        _outbox.Writer.Complete();
        await _worker.Stop();
    }

    private async Task Run()
    {
        _output ??= PipeWriter.Create(await _transportProvider.GetOutputStream());

        while (await _outbox.Reader.WaitToReadAsync())
        while (_outbox.Reader.TryRead(out var message))
        {
            var payload = _serializer.ObjectToBytes(message);
            var header = Encoding.UTF8.GetBytes($"Content-Length: {payload.Length}\r\n\r\n");

            await _output.WriteAsync(header);
            await _output.WriteAsync(payload);
                
            if (_config.TraceTarget != null)
                await _config.TraceTarget.OnOutgoingMessage(message, _serializer);
        }
    }
}