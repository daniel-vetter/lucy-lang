using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer;

public interface IJsonRpcMessageTraceTarget : IAsyncDisposable
{
    Task Initialize();
    Task OnIncomingMessage(Message message, JsonRpcSerializer serializer);
    Task OnOutgoingMessage(Message message, JsonRpcSerializer serializer);
}

public class DiskTraceTarget : IJsonRpcMessageTraceTarget
{
    private readonly string _file;
    private readonly Channel<string> _channel = Channel.CreateUnbounded<string>();
    private readonly Worker _worker = new Worker();

    public DiskTraceTarget(string file)
    {
        _file = file;
    }

    public async ValueTask DisposeAsync()
    {
        await _worker.Stop();
    }

    public Task Initialize()
    {
        _worker.Start(ProcessMessages);
        return Task.CompletedTask;
    }

    private async Task ProcessMessages(CancellationToken ct)
    {
        if (File.Exists(_file))
            File.Delete(_file);
        using var file = new FileStream(_file, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
        using var stringWriter = new StreamWriter(file);

        while (await _channel.Reader.WaitToReadAsync(ct))
        while (_channel.Reader.TryRead(out var message))
        {
            await stringWriter.WriteLineAsync(message);
            await stringWriter.FlushAsync();
        }

        stringWriter.Close();
        file.Close();
    }

    public async Task OnIncomingMessage(Message message, JsonRpcSerializer serializer)
    {
        var type = message switch
        {
            NotificationMessage => "recv-notification",
            RequestMessage => "recv-request",
            ResponseSuccessMessage => "recv-response",
            ResponseErrorMessage => "recv-response",
            _ => throw new Exception("Unsupported type: " + message.GetType().Name)
        };

        var c = new Container(type, message, DateTimeOffset.Now.ToUnixTimeMilliseconds());
        await _channel.Writer.WriteAsync(serializer.ObjectToString(c));
    }

    public async Task OnOutgoingMessage(Message message, JsonRpcSerializer serializer)
    {
        var type = message switch
        {
            NotificationMessage => "send-notification",
            RequestMessage => "send-request",
            ResponseSuccessMessage => "send-response",
            ResponseErrorMessage => "send-response",
            _ => throw new Exception("Unsupported type: " + message.GetType().Name)
        };

        var c = new Container(type, message, DateTimeOffset.Now.ToUnixTimeMilliseconds());
        await _channel.Writer.WriteAsync(serializer.ObjectToString(c));
    }
}

public record Container(string Type, Message Message, long Timestamp);