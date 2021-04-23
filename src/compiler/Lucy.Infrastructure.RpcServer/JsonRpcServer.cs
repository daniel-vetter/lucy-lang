using System;
using System.Threading;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer.Internal;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer
{
    public class JsonRpcServer
    {
        private Running? _running;

        public async Task Start(JsonRpcConfig config)
        {
            if (_running != null)
                throw new Exception("Server is already running");

            var incommingMessageReader = new IncommingMessageReader(config.TraceTarget);
            var outgoingMessageWriter = new OutgoingMessageWriter(config.TraceTarget);
            var outgoingRequestHandler = new OutgoingRequestHandler(outgoingMessageWriter);
            var incommingMessageHandler = new IncommingMessageHandler(config, incommingMessageReader, outgoingMessageWriter, outgoingRequestHandler);

            _running = new Running(
                config,
                incommingMessageReader,
                outgoingMessageWriter,
                incommingMessageHandler,
                outgoingRequestHandler,
                new TaskCompletionSource()
            );

            if (config.TraceTarget != null)
                await config.TraceTarget.Initialize();

            outgoingMessageWriter.Start();
            incommingMessageHandler.Start();
        }

        public async Task Stop()
        {
            var running = _running;
            if (running == null)
                throw new Exception("Server was not started");

            try
            {
                await running.MessageHandler.Stop();
                await running.OutgoingMessageWriter.Stop();
            }
            finally
            {
                _running = null;
                running.FullRuntimeTask.SetResult();
            }
        }

        public async Task SendNotification(string name, object? parameter)
        {
            if (_running == null)
                throw new Exception("Can not send a notifiction because the connection is closed");

            await _running.OutgoingRequestHandler.SendNotification(name, parameter);

        }

        public async Task<TResult> SendRequest<TResult>(string name, object? parameter)
        {
            if (_running == null)
                throw new Exception("Can not send a request because the connection is closed");

            return await _running.OutgoingRequestHandler.SendRequest<TResult>(name, parameter);
        }

        public async Task WaitTillStopped()
        {
            var running = _running;
            if (running == null)
                throw new Exception("Server was not started");

            await running.FullRuntimeTask.Task;
        }

        private record Running(
            JsonRpcConfig Config,
            IncommingMessageReader IncommingMessageReader,
            OutgoingMessageWriter OutgoingMessageWriter,
            IncommingMessageHandler MessageHandler,
            OutgoingRequestHandler OutgoingRequestHandler,
            TaskCompletionSource FullRuntimeTask
        );
    }

    internal class IncommingMessageHandler
    {
        private readonly IncommingMessageReader _incommingMessageReader;
        private readonly OutgoingRequestHandler _outgoingRequestHandler;
        private readonly JobRunner _jobRunner;
        private readonly IncommingRequestHandler _incommingRequestHandler;
        private readonly Worker _worker = new Worker();

        public IncommingMessageHandler(JsonRpcConfig config, IncommingMessageReader incommingMessageReader, OutgoingMessageWriter outgoingMessageWriter, OutgoingRequestHandler outgoingRequestHandler)
        {
            _incommingMessageReader = incommingMessageReader;
            _outgoingRequestHandler = outgoingRequestHandler;
            _jobRunner = new JobRunner();
            _incommingRequestHandler = new IncommingRequestHandler(config, outgoingMessageWriter, _jobRunner);
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

    internal class OutgoingRequestHandler
    {
        private readonly OutgoingRequestTracker _outgoingRequestTracker;
        private readonly OutgoingMessageWriter _outgoingMessageWriter;

        public OutgoingRequestHandler(OutgoingMessageWriter outgoingMessageWriter)
        {
            _outgoingRequestTracker = new OutgoingRequestTracker();
            _outgoingMessageWriter = outgoingMessageWriter;
        }

        public async Task<TResult> SendRequest<TResult>(string name, object? parameter)
        {
            var tracker = _outgoingRequestTracker.CreateNew<TResult>();
            await _outgoingMessageWriter.Write(new RequestMessage(tracker.Id, name, Serializer.ObjectToToken(parameter)));
            return await tracker.ResponseTask;
        }

        public async Task SendNotification(string name, object? parameter)
        {
            await _outgoingMessageWriter.Write(new NotificationMessage(name, Serializer.ObjectToToken(parameter)));
        }

        public void HandleResponse(ResponseMessage response)
        {
            var type = _outgoingRequestTracker.GetRequestResultType(response.Id);
            if (type == null)
                return; //TODO: Logging

            if (response is ResponseSuccessMessage success)
                _outgoingRequestTracker.SetResult(success.Id, Serializer.TokenToObject(success.Result, type));

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
}
