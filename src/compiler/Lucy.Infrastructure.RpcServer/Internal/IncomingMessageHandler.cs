using System.Diagnostics;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Lucy.Infrastructure.RpcServer.Internal;

[Service(Lifetime.Singleton)]
public class IncomingMessageHandler
{
    private readonly FunctionCaller _functionCaller;
    private readonly ILogger<IncomingMessageHandler> _logger;
    private readonly FunctionFinder _functionFinder;
    private readonly OutgoingMessageWriter _outgoingMessageWriter;
    private readonly JsonRpcSerializer _serializer;
    private readonly Channel<Message> _inbox = Channel.CreateUnbounded<Message>();
    private readonly Worker _worker = new();

    public IncomingMessageHandler(OutgoingMessageWriter outgoingMessageWriter, JsonRpcSerializer serializer, FunctionFinder functionFinder, FunctionCaller functionCaller, ILogger<IncomingMessageHandler> logger)
    {
        _outgoingMessageWriter = outgoingMessageWriter;
        _serializer = serializer;
        _functionFinder = functionFinder;
        _functionCaller = functionCaller;
        _logger = logger;
    }

    public void HandleNotification(NotificationMessage notification)
    {
        _inbox.Writer.WriteAsync(notification);
    }

    public void HandleRequest(RequestMessage request)
    {
        _inbox.Writer.WriteAsync(request);
    }

    public void Start() => _worker.Start(_ => Run());

    public async Task Stop()
    {
        _inbox.Writer.Complete();
        await _worker.Stop();
    }

    private async Task Run()
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

        var sw = Stopwatch.StartNew();
        var result = await _functionCaller.Call(function, notificationMessage.Params);

        if (result.Error != null)
        {
            _logger.LogError(result.Error, "Handler failed while processing a incoming '{type}' notification. Processing took {duration}ms", notificationMessage.Method, sw.Elapsed.Milliseconds);
        }
        else
        {
            _logger.LogInformation("Successfully handled incoming '{type}' notification. Processing took {duration}ms", notificationMessage.Method, sw.Elapsed.Milliseconds);
        }

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

            _logger.LogWarning("Could not find a handler for the incoming '{type}' request.", requestMessage.Method);
            return;
        }

        var sw = Stopwatch.StartNew();
        var result = await _functionCaller.Call(function, requestMessage.Params);
        if (result.Error != null)
        {
            await _outgoingMessageWriter.Write(new ResponseErrorMessage(requestMessage.Id, new ErrorDescription(-1, result.Error.Message)));
            _logger.LogError(result.Error, "Handler failed while processing a incoming '{type}' request. Processing took {duration}ms", requestMessage.Method, sw.Elapsed.Milliseconds);
            return;
        }
        else
        {
            _logger.LogInformation("Successfully handled incoming '{type}' request. Processing took {duration}ms", requestMessage.Method, sw.Elapsed.Milliseconds);
        }

        await _outgoingMessageWriter.Write(new ResponseSuccessMessage(requestMessage.Id, _serializer.ObjectToToken(result.Result)));
    }
}