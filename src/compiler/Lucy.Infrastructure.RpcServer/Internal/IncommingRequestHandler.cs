using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

namespace Lucy.Infrastructure.RpcServer.Internal
{
    class IncommingRequestHandler
    {
        private readonly FunctionCaller _functionCaller;
        private readonly FunctionFinder _functionFinder;
        private readonly OutgoingMessageWriter _outgoingMessageWriter;
        private readonly JobRunner _jobRunner;
        private readonly Channel<Message> _inbox = Channel.CreateUnbounded<Message>();
        private readonly Worker _worker = new Worker();

        public IncommingRequestHandler(JsonRpcConfig config, OutgoingMessageWriter outgoingMessageWriter, JobRunner jobRunner)
        {
            _functionCaller = new FunctionCaller();
            _functionFinder = new FunctionFinder(config);
            _outgoingMessageWriter = outgoingMessageWriter;
            _jobRunner = jobRunner;
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

            await _outgoingMessageWriter.Write(new ResponseSuccessMessage(requestMessage.Id, Serializer.ObjectToToken(result.Result)));
        }
    }
}
