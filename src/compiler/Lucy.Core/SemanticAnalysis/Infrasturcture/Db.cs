using Lucy.Common;
using Lucy.Core.SemanticAnalysis.Handler;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lucy.Core.SemanticAnalysis.Infrasturcture
{
    public class Db
    {
        private Dictionary<IQuery, Entry> _entries = new();
        private Dictionary<Type, QueryHandler> _handlers = new();
        private Subscriptions<IDbEvent> _subscriptions = new();
        private int _currentRevision;
        //TODO: Garbage collection

        public void RegisterHandler(QueryHandler handler)
        {
            _handlers.Add(handler.GetType().BaseType!.GetGenericArguments()[0], handler);
        }

        public IDisposable AddEventHandler(Action<IDbEvent> handler)
        {
            return _subscriptions.AddHandler(handler);
        }

        public void SetInput<TQueryResult>(IQuery<TQueryResult> query, TQueryResult result) where TQueryResult : notnull
        {
            _currentRevision++;
            if (_entries.TryGetValue(query, out var existingEntry))
            {
                if (!existingEntry.IsInput)
                    throw new Exception("The result of this query was already set by an query handle. It can not be changed to an input.");

                existingEntry.LastChanged = _currentRevision;
                existingEntry.Result = result;
            }
            else
            {
                _entries.Add(query, new Entry(result, _currentRevision, _currentRevision, true));
            }

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new InputWasChanged(query, result));
        }

        public void RemoveInput<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
        {
            if (!_entries.Remove(query))
                throw new Exception("The input could not be removed because it did not exist.");

            _currentRevision++;

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new InputWasRemoved(query));
        }

        private Entry EnsureEntryIsUpToDate(IQuery query)
        {
            if (_entries.TryGetValue(query, out var entry) && entry.LastChecked == _currentRevision)
                return entry;

            while (true)
            {
                var outOfData = GetFirstOutOfDateEntry(query);
                if (outOfData == null)
                    break;

                Recalculate(outOfData);
            }

            return _entries[query];
        }

        private IQuery? GetFirstOutOfDateEntry(IQuery query)
        {
            if (!_entries.TryGetValue(query, out var entry))
                return query;

            if (entry.LastChecked == _currentRevision)
                return null;

            for (int i=0;i<entry.Dependencies.Count;i++)
            {
                if (!_entries.TryGetValue(entry.Dependencies[i], out var dependencyEntry) || dependencyEntry.LastChanged > entry.LastChanged)
                    return query;
            }

            foreach(var dep in entry.Dependencies)
            {
                var match = GetFirstOutOfDateEntry(dep);
                if (match != null)
                    return match;
            }

            entry.LastChecked = _currentRevision;
            return null;
        }

        public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
        {
            return (TQueryResult)Query(query, null);
        }

        private object Query(IQuery query, IQuery? parentQuery)
        {
            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new QueryReceived(query, parentQuery));

            var entry = EnsureEntryIsUpToDate(query);

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new QueryAnswered(query, parentQuery));

            return entry.Result;
        }

        private Entry Recalculate(IQuery query)
        {
            if (!_handlers.TryGetValue(query.GetType(), out var handler))
                throw new Exception($"For a query of type '{query.GetType().Name}' is no input provided and no query handler registered.");

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new CalculationStarted(query));

            var callContext = new QueryExecutionContext(this, query);
            var handlerStopwatch = Stopwatch.StartNew();
            var result = handler.Handle(callContext, query);
            handlerStopwatch.Stop();


            ResultType resultType;
            _entries.TryGetValue(query, out var entry);
            var overheadStopwatch = Stopwatch.StartNew();
            if (entry == null)
            {
                entry = new Entry(result, _currentRevision, _currentRevision, false, callContext.Dependencies); ;
                _entries[query] = entry;
                resultType = ResultType.InitialCalculation;
            }
            else
            {
                if (entry.IsInput)
                    throw new Exception("The result of this query was already set as an input. It can not be changed to an result of an query handler.");

                if (!result.Equals(entry.Result))
                {
                    resultType = ResultType.HasChanged;
                    entry.LastChanged = _currentRevision;
                    entry.Result = result;
                }
                else
                {
                    resultType = ResultType.WasTheSame;
                }
                entry.Dependencies = callContext.Dependencies;
                entry.LastChecked = _currentRevision;
            }
            overheadStopwatch.Stop();

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new CalculationFinished(query, entry.Result, handlerStopwatch.Elapsed - callContext.TotalTimeInSubQueries, overheadStopwatch.Elapsed, resultType));

            return entry;
        }

        private class Entry
        {
            public Entry(object result, int lastChanged, int lastChecked, bool isInput, List<IQuery>? dependencies = null)
            {
                Result = result;
                LastChanged = lastChanged;
                LastChecked = lastChecked;
                IsInput = isInput;
                Dependencies = dependencies ?? new List<IQuery>();
            }

            public int LastChanged { get; set; }
            public int LastChecked { get; set; }
            public bool IsInput { get; }
            public List<IQuery> Dependencies { get; set; }
            public object Result { get; set; }
        }

        private class QueryExecutionContext : IDb
        {
            public QueryExecutionContext(Db db, IQuery parentQuery)
            {
                _db = db;
                _parentQuery = parentQuery;
            }

            public List<IQuery> Dependencies = new();
            public TimeSpan TotalTimeInSubQueries => _totalTimeInSubQueries;

            private IQuery _parentQuery;
            private readonly Db _db;
            private TimeSpan _totalTimeInSubQueries = TimeSpan.Zero;

            public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
            {
                Dependencies.Add(query);
                var sw = Stopwatch.StartNew();
                var result = (TQueryResult)_db.Query(query, _parentQuery);
                sw.Stop();
                _totalTimeInSubQueries = _totalTimeInSubQueries + sw.Elapsed;
                return result;
            }
        }
    }
}
