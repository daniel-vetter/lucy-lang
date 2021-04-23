using System;
using System.Collections.Generic;

namespace Lucy.Testing.Internal
{
    internal static class Progress
    {
        private static List<Action<TestProgress>> _subscriber = new List<Action<TestProgress>>();
        private static List<TestProgress> _updates = new List<TestProgress>();

        public static void Publish(TestProgress progress)
        {
            Action<TestProgress>[] subscriptions;
            lock (_subscriber)
                subscriptions = _subscriber.ToArray();

            lock (_updates)
                _updates.Add(progress);

            foreach (var action in subscriptions)
                action(progress);
        }

        public static IDisposable OnProgress(Action<TestProgress> onProgressUpdate)
        {
            lock (_subscriber)
                _subscriber.Add(onProgressUpdate);

            lock (_updates)
                foreach (var update in _updates)
                    onProgressUpdate(update);

            return new Disposable(() =>
            {
                lock (_subscriber)
                    _subscriber.Remove(onProgressUpdate);
            });
        }
    }
}
