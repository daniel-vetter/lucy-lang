using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer.Internal;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Lucy.Infrastructure.RpcServer;

[Service(Lifetime.Singleton)]
public class JsonRpcServer
{
    private bool _running;
    private readonly OutgoingMessageWriter _outgoingMessageWriter;
    private readonly IncomingMessageHandler _incomingMessageHandler;
    private readonly OutgoingRequestHandler _outgoingRequestHandler;
    private readonly IEnumerable<IJsonRpcMessageTraceTarget> _traceTargets;
    private readonly ILogger<JsonRpcServer> _logger;
    private readonly TaskCompletionSource _isDone = new();

    public JsonRpcServer(
        OutgoingMessageWriter outgoingMessageWriter,
        IncomingMessageHandler incomingMessageHandler,
        OutgoingRequestHandler outgoingRequestHandler,
        IEnumerable<IJsonRpcMessageTraceTarget> traceTargets,
        ILogger<JsonRpcServer> logger
    )
    {
        _outgoingMessageWriter = outgoingMessageWriter;
        _incomingMessageHandler = incomingMessageHandler;
        _outgoingRequestHandler = outgoingRequestHandler;
        _traceTargets = traceTargets;
        _logger = logger;
    }

    public async Task Start()
    {
        if (_running)
            throw new Exception("Server is already running");

        foreach (var traceTarget in _traceTargets)
            await traceTarget.Initialize();

        _outgoingMessageWriter.Start();
        _incomingMessageHandler.Start();
        _running = true;

        _logger.LogInformation("Json RPC server started.");
    }

    public async Task Stop()
    {
        var running = _running;
        if (!running)
            throw new Exception("Server was not started");

        try
        {
            await _incomingMessageHandler.Stop();
            await _outgoingMessageWriter.Stop();
        }
        finally
        {
            _running = false;
            _isDone.SetResult();

            _logger.LogInformation("Json RPC server stopped.");
        }
    }

    public async Task SendNotification(string name, object? parameter)
    {
        if (!_running)
            throw new Exception("Can not send a notification because the connection is closed");

        await _outgoingRequestHandler.SendNotification(name, parameter);

    }

    public async Task<TResult> SendRequest<TResult>(string name, object? parameter)
    {
        if (!_running)
            throw new Exception("Can not send a request because the connection is closed");

        return await _outgoingRequestHandler.SendRequest<TResult>(name, parameter);
    }

    public async Task WaitTillStopped()
    {
        if (!_running)
            throw new Exception("Server was not started");

        await _isDone.Task;
    }
}

[Service(Lifetime.Singleton)]
public class IncomingMessageHandler
{
    private readonly IncomingMessageReader _incomingMessageReader;
    private readonly OutgoingRequestHandler _outgoingRequestHandler;
    private readonly JobRunner _jobRunner;
    private readonly Internal.IncomingMessageHandler _incomingMessageHandler;
    private readonly Worker _worker = new();

    public IncomingMessageHandler(Internal.IncomingMessageHandler incomingMessageHandler, IncomingMessageReader incomingMessageReader, OutgoingMessageWriter outgoingMessageWriter, OutgoingRequestHandler outgoingRequestHandler, JsonRpcSerializer serializer, JobRunner jobRunner)
    {
        _incomingMessageReader = incomingMessageReader;
        _outgoingRequestHandler = outgoingRequestHandler;
        _jobRunner = jobRunner;
        _incomingMessageHandler = incomingMessageHandler;
    }

    public void Start()
    {
        _incomingMessageHandler.Start();
        _worker.Start(Process);
    }

    public async Task Stop()
    {
        await _worker.Stop();
        await _incomingMessageHandler.Stop();
    }

    private async Task Process(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var message = await _incomingMessageReader.ReadNext(ct);
            if (message == null)
                return;

            if (message is RequestMessage requestMessage)
                _incomingMessageHandler.HandleRequest(requestMessage);
            if (message is NotificationMessage notificationMessage)
                _incomingMessageHandler.HandleNotification(notificationMessage);
            if (message is ResponseMessage responseMessage)
                _outgoingRequestHandler.HandleResponse(responseMessage);
        }

        await _jobRunner.CloseAndWaitTillAllJobsAreDone();
    }
}

[Service(Lifetime.Singleton)]
public class OutgoingRequestHandler
{
    private readonly OutgoingRequestTracker _outgoingRequestTracker;
    private readonly ILogger<OutgoingRequestHandler> _logger;
    private readonly OutgoingMessageWriter _outgoingMessageWriter;
    private readonly JsonRpcSerializer _serializer;

    public OutgoingRequestHandler(OutgoingMessageWriter outgoingMessageWriter, JsonRpcSerializer serializer, OutgoingRequestTracker outgoingRequestTracker, ILogger<OutgoingRequestHandler> logger)
    {
        _outgoingRequestTracker = outgoingRequestTracker;
        _logger = logger;
        _outgoingMessageWriter = outgoingMessageWriter;
        _serializer = serializer;
    }

    public async Task<TResult> SendRequest<TResult>(string name, object? parameter)
    {
        var tracker = _outgoingRequestTracker.CreateNew<TResult>();
        _logger.LogInformation("Sending request '{type}'.", name);
        await _outgoingMessageWriter.Write(new RequestMessage(tracker.Id, name, _serializer.ObjectToToken(parameter)));
        return await tracker.ResponseTask;
    }

    public async Task SendNotification(string name, object? parameter)
    {
        _logger.LogInformation("Sending notification '{type}'.", name);
        await _outgoingMessageWriter.Write(new NotificationMessage(name, _serializer.ObjectToToken(parameter)));
    }

    public void HandleResponse(ResponseMessage response)
    {
        var type = _outgoingRequestTracker.GetRequestResultType(response.Id);
        if (type == null)
        {
            _logger.LogError("Received a response message with id {id} but no corresponding request message was send out.", response.Id);
            return;
        }

        if (response is ResponseSuccessMessage success)
            _outgoingRequestTracker.SetResult(success.Id, _serializer.TokenToObject(success.Result, type));

        if (response is ResponseErrorMessage error)
            _outgoingRequestTracker.SetResult(error.Id, new JsonRpcException(error.Error.Code, error.Error.Message));
    }
}

public class JsonRpcException : Exception
{
    public JsonRpcException(int code, string message) : base(message)
    {
        Code = code;
    }

    public int Code { get; }
}