using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

internal class Worker
{
    private readonly object _syncObj = new();
    private Task? _task;
    private CancellationTokenSource? _cts;

    public void Start(Func<CancellationToken, Task> handler)
    {
        if (_task != null)
            throw new Exception("Worker was already started.");

        lock (_syncObj)
        {
            if (_task != null)
                throw new Exception("Worker was already started.");

            _cts = new CancellationTokenSource();
            _task = Task.Run(async () => await handler(_cts.Token));
        }
    }

    public async Task Stop()
    {
        if (_task == null || _cts == null)
            throw new Exception("Worker was not started.");

        Task task;
        CancellationTokenSource cts;

        lock (_syncObj)
        {
            if (_task == null || _cts == null)
                throw new Exception("Worker was not started.");

            task = _task;
            cts = _cts;

            _task = null;
            _cts = null;
        }

        try
        {
            cts.Cancel();
            await task;
        }
        catch (Exception)
        {
            //TODO: Logger
        }
    }
}