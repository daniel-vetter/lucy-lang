using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer.Internal;

[Service(Lifetime.Singleton)]
public class IncommingRequestHandler
{
    private readonly FunctionCaller _functionCaller;
    private readonly FunctionFinder _functionFinder;
    private readonly OutgoingMessageWriter _outgoingMessageWriter;
    private readonly JsonRpcSerializer _serializer;
    private readonly Channel<Message> _inbox = Channel.CreateUnbounded<Message>();
    private readonly Worker _worker = new Worker();

    public IncommingRequestHandler(OutgoingMessageWriter outgoingMessageWriter, JsonRpcSerializer serializer, FunctionFinder functionFinder, FunctionCaller functionCaller)
    {
        _outgoingMessageWriter = outgoingMessageWriter;
        _serializer = serializer;
        _functionFinder = functionFinder;
        _functionCaller = functionCaller;
    }

    public void HandleNotificion(NotificationMessage notification)
    {
        _inbox.Writer.WriteAsync(notification);
    }

    public void HandleRequest(RequestMessage request)
    {
        _inbox.Writer.WriteAsync(request);
    }

    public void Start() => _worker.Start(Run);

    public async Task Stop()
    {
        _inbox.Writer.Complete();
        await _worker.Stop();
    }

    private async Task Run(CancellationToken arg)
    {
        while (await _inbox.Reader.WaitToReadAsync())
        while (_inbox.Reader.TryRead(out var message))
        {
            if (message is NotificationMessage notificationMessage)
                await ProcessNotification(notificationMessage);

            if (message is RequestMessage requestMessage)
                await ProcessRequest(requestMessage);
        }
    }

    private async Task ProcessNotification(NotificationMessage notificationMessage)
    {
        var function = _functionFinder.Find(notificationMessage.Method);
        if (function == null)
            return;

        await _functionCaller.Call(function, notificationMessage.Params);
    }

    private async Task ProcessRequest(RequestMessage requestMessage)
    {
        var function = _functionFinder.Find(requestMessage.Method);
        if (function == null)
        {
            await _outgoingMessageWriter.Write(new ResponseErrorMessage(
                Id: requestMessage.Id,
                Error: new ErrorDescription(-32601, $"The method '{requestMessage.Method}' does not exist.")
            ));
            return;
        }

        var result = await _functionCaller.Call(function, requestMessage.Params);
        if (result.Error != null)
        {
            await _outgoingMessageWriter.Write(new ResponseErrorMessage(requestMessage.Id, new ErrorDescription(-1, result.Error.Message)));
            return;
        }

        await _outgoingMessageWriter.Write(new ResponseSuccessMessage(requestMessage.Id, _serializer.ObjectToToken(result.Result)));
    }
}