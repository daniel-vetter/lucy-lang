using System;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer.Internal
{
    internal class OutgoingMessageWriter
    {
        private readonly PipeWriter _output;
        private readonly Channel<Message> _outbox = Channel.CreateUnbounded<Message>();
        private readonly Worker _worker = new Worker();
        private readonly IJsonRpcMessageTrace? _trace;

        public OutgoingMessageWriter(IJsonRpcMessageTrace? trace)
        {
            _output = PipeWriter.Create(Console.OpenStandardOutput());
            _trace = trace;
        }

        public async ValueTask Write(Message message)
        {
            await _outbox.Writer.WriteAsync(message);
        }

        public void Start() => _worker.Start(Run);
        public async Task Stop()
        {
            _outbox.Writer.Complete();
            await _worker.Stop();
        }

        private async Task Run(CancellationToken arg)
        {
            while (await _outbox.Reader.WaitToReadAsync())
                while (_outbox.Reader.TryRead(out var message))
                {
                    var payload = Serializer.ObjectToBytes(message);
                    var header = Encoding.UTF8.GetBytes($"Content-Length: {payload.Length}\r\n\r\n");

                    await _output.WriteAsync(header);
                    await _output.WriteAsync(payload);

                    if (_trace != null)
                        await _trace.OnOutgoingMessage(message);
                }
        }
    }
}