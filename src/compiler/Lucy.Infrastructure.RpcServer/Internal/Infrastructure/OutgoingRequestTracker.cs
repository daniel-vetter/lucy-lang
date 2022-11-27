using Lucy.Common.ServiceDiscovery;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

[Service(Lifetime.Singleton)]
public class OutgoingRequestTracker
{
    private int _lastId = -1;

    private readonly Dictionary<long, ActiveRequest> _activeRequests = new();

    public OutgoingRequest<T> CreateNew<T>()
    {
        var id = Interlocked.Increment(ref _lastId);
        var taskCompletionSource = new TaskCompletionSource<T>();

        var ar = new ActiveRequest(id, typeof(T), x => taskCompletionSource.SetResult((T)x!), x => taskCompletionSource.SetException(x));
            
        lock (_activeRequests)
            _activeRequests.Add(ar.Id, ar);

        return new OutgoingRequest<T>(ar.Id, taskCompletionSource.Task);
    }

    public bool SetResult(long id, object? result)
    {
        if (!GetActiveRequest(id, out var ar))
            throw new Exception($"No active request with {id} found.");

        ar.SetResult(result);
        return true;
    }

    public bool SetException(long id, Exception exception)
    {
        if (!GetActiveRequest(id, out var ar))
            throw new Exception($"No active request with {id} found.");

        ar.SetException(exception);
        return true;
    }

    internal Type? GetRequestResultType(long id)
    {
        return GetActiveRequest(id, out var ar) ? ar.ResultType : null;
    }

    private bool GetActiveRequest(long id, [NotNullWhen(true)] out ActiveRequest? activeRequest)
    {
        lock (_activeRequests)
        {
            return _activeRequests.TryGetValue(id, out activeRequest);
        }
    }

    private class ActiveRequest
    {
        private readonly Action<object?> _onSetResult;
        private readonly Action<Exception> _onSetException;

        public ActiveRequest(long id, Type resultType, Action<object?> onSetResult, Action<Exception> onSetException)
        {
            Id = id;
            ResultType = resultType;
            _onSetResult = onSetResult;
            _onSetException = onSetException;
        }

        public long Id { get; private set; }
        public Type ResultType { get; }

        public void SetResult(object? result) => _onSetResult(result);
        public void SetException(Exception exception) => _onSetException(exception);
    }
}

public record OutgoingRequest<T>(long Id, Task<T> ResponseTask);