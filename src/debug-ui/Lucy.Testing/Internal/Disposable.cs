using System;

namespace Lucy.Testing.Internal
{
    internal class Disposable : IDisposable
    {
        Action _onDispose;

        public Disposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose();
        }
    }
}
