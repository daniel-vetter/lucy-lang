using System;
using System.Collections.Generic;

namespace Lucy.Common;

public class Subscriptions<T>
{
    readonly List<Action<T>> _handler = new List<Action<T>>();

    public IDisposable AddHandler(Action<T> handler)
    {
        _handler.Add(handler);
        return new DelegateDisposable(() => _handler.Remove(handler));
    }

    public void Publish(T @event)
    {
        foreach(var handler in _handler)
        {
            handler(@event);
        }
    }

    private class DelegateDisposable : IDisposable
    {
        private readonly Action _onDispose;
        public DelegateDisposable(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose();
    }

    public bool HasSubscriptions => _handler.Count > 0;
}