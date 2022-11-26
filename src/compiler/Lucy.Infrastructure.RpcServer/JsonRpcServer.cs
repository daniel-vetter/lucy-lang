using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Lucy.Infrastructure.RpcServer.Internal;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer;

[Service(Lifetime.Singleton)]
public class JsonRpcServer
{
    private bool _running;
    private readonly OutgoingMessageWriter _outgoingMessageWriter;
    private readonly IncommingMessageHandler _incommingMessageHandler;
    private readonly OutgoingRequestHandler _outgoingRequestHandler;
    private readonly IEnumerable<IJsonRpcMessageTraceTarget> _traceTargets;
    private readonly TaskCompletionSource _isDone = new TaskCompletionSource();

    public JsonRpcServer(
        OutgoingMessageWriter outgoingMessageWriter,
        IncommingMessageHandler incommingMessageHandler,
        OutgoingRequestHandler outgoingRequestHandler,
        IEnumerable<IJsonRpcMessageTraceTarget> traceTargets
    )
    {
        _outgoingMessageWriter = outgoingMessageWriter;
        _incommingMessageHandler = incommingMessageHandler;
        _outgoingRequestHandler = outgoingRequestHandler;
        _traceTargets = traceTargets;
    }

    public async Task Start()
    {
        if (_running)
            throw new Exception("Server is already running");

        foreach(var traceTarget in _traceTargets)
            await traceTarget.Initialize();

        _outgoingMessageWriter.Start();
        _incommingMessageHandler.Start();
        _running = true;
    }

    public async Task Stop()
    {
        var running = _running;
        if (!running)
            throw new Exception("Server was not started");

        try
        {
            await _incommingMessageHandler.Stop();
            await _outgoingMessageWriter.Stop();
        }
        finally
        {
            _running = false;
            _isDone.SetResult();
        }
    }

    public async Task SendNotification(string name, object? parameter)
    {
        if (!_running)
            throw new Exception("Can not send a notifiction because the connection is closed");

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
public class IncommingMessageHandler
{
    private readonly IncommingMessageReader _incommingMessageReader;
    private readonly OutgoingRequestHandler _outgoingRequestHandler;
    private readonly JobRunner _jobRunner;
    private readonly IncommingRequestHandler _incommingRequestHandler;
    private readonly Worker _worker = new Worker();

    public IncommingMessageHandler(JsonRpcConfig config, IncommingRequestHandler incommingRequestHandler,  IncommingMessageReader incommingMessageReader, OutgoingMessageWriter outgoingMessageWriter, OutgoingRequestHandler outgoingRequestHandler, JsonRpcSerializer serializer, JobRunner jobRunner)
    {
        _incommingMessageReader = incommingMessageReader;
        _outgoingRequestHandler = outgoingRequestHandler;
        _jobRunner = jobRunner;
        _incommingRequestHandler = incommingRequestHandler;
    }

    public void Start()
    {
        _incommingRequestHandler.Start();
        _worker.Start(Process);
    }

    public async Task Stop()
    {
        await _worker.Stop();
        await _incommingRequestHandler.Stop();
    }

    private async Task Process(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            var message = await _incommingMessageReader.ReadNext(ct);
            if (message == null)
                return;

            if (message is RequestMessage requestMessage)
                _incommingRequestHandler.HandleRequest(requestMessage);
            if (message is NotificationMessage notificationMessage)
                _incommingRequestHandler.HandleNotificion(notificationMessage);
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
    private readonly OutgoingMessageWriter _outgoingMessageWriter;
    private readonly JsonRpcSerializer _serializer;

    public OutgoingRequestHandler(OutgoingMessageWriter outgoingMessageWriter, JsonRpcSerializer serializer, OutgoingRequestTracker outgoingRequestTracker)
    {
        _outgoingRequestTracker = outgoingRequestTracker;
        _outgoingMessageWriter = outgoingMessageWriter;
        _serializer = serializer;
    }

    public async Task<TResult> SendRequest<TResult>(string name, object? parameter)
    {
        var tracker = _outgoingRequestTracker.CreateNew<TResult>();
        await _outgoingMessageWriter.Write(new RequestMessage(tracker.Id, name, _serializer.ObjectToToken(parameter)));
        return await tracker.ResponseTask;
    }

    public async Task SendNotification(string name, object? parameter)
    {
        await _outgoingMessageWriter.Write(new NotificationMessage(name, _serializer.ObjectToToken(parameter)));
    }

    public void HandleResponse(ResponseMessage response)
    {
        var type = _outgoingRequestTracker.GetRequestResultType(response.Id);
        if (type == null)
            return; //TODO: Logging

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

    public int Code { get; private set; }
}