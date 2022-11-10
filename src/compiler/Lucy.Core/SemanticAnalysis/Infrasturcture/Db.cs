using Lucy.Common;
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
                _entries.Add(query, new Entry(query, result, _currentRevision, _currentRevision, true, null));
            }

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new InputWasChanged(query, result));
        }

        public void RemoveInput<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
        {
            if (!_entries.Remove(query, out var entry))
                throw new Exception("The input could not be removed because it did not exist.");

            _currentRevision++;
            entry.LastChanged = _currentRevision;

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new InputWasRemoved(query));
        }

        private void EnsureEntryIsUpToDate(Entry entry)
        {
            if (entry.LastChecked == _currentRevision)
                return;

            while (true)
            {
                var outOfDate = GetFirstOutOfDateEntry(entry);
                if (outOfDate == null)
                    break;

                Recalculate(outOfDate);
            }
        }

        private Entry? GetFirstOutOfDateEntry(Entry entry)
        {
            if (entry.LastChecked == _currentRevision)
            {
                return null;
            }
            
            for (int i=0;i<entry.Dependencies.Count;i++)
            {
                if (entry.Dependencies[i].LastChanged > entry.LastChanged)
                    return entry;
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
            return (TQueryResult)(Query(query, null).Result ?? throw new Exception("Query was not executed."));
        }

        private Entry Query(IQuery query, IQuery? parentQuery)
        {
            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new QueryReceived(query, parentQuery));

            if (!_entries.TryGetValue(query, out var entry))
            {
                entry = new Entry(query, null, 0, 0, false, new List<Entry>());
                _entries[query] = entry;
                Recalculate(entry);
            }

            EnsureEntryIsUpToDate(entry);

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new QueryAnswered(query, parentQuery));

            return entry;
        }

        private Entry Recalculate(Entry entry)
        {
            if (!_handlers.TryGetValue(entry.Query.GetType(), out var handler))
                throw new Exception($"For a query of type '{entry.Query.GetType().Name}' is no input provided and no query handler registered.");

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new CalculationStarted(entry.Query));

            var callContext = new QueryExecutionContext(this, entry.Query);
            var handlerStopwatch = Stopwatch.StartNew();
            var result = handler.Handle(callContext, entry.Query);
            handlerStopwatch.Stop();


            ResultType resultType;
            var overheadStopwatch = Stopwatch.StartNew();
            
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
            
            overheadStopwatch.Stop();

            if (_subscriptions.HasSubscriptions)
                _subscriptions.Publish(new CalculationFinished(entry.Query, entry.Result, handlerStopwatch.Elapsed - callContext.TotalTimeInSubQueries, handlerStopwatch.Elapsed, overheadStopwatch.Elapsed, resultType));

            return entry;
        }

        private class Entry
        {
            public Entry(IQuery query, object? result, int lastChanged, int lastChecked, bool isInput, List<Entry>? dependencies = null)
            {
                Query = query;
                Result = result;
                LastChanged = lastChanged;
                LastChecked = lastChecked;
                IsInput = isInput;
                Dependencies = dependencies ?? new List<Entry>();
            }

            public int LastChanged { get; set; }
            public int LastChecked { get; set; }
            public bool IsInput { get; }
            public List<Entry> Dependencies { get; set; }
            public IQuery Query { get; }
            public object? Result { get; set; }
        }

        private class QueryExecutionContext : IDb
        {
            public QueryExecutionContext(Db db, IQuery parentQuery)
            {
                _db = db;
                _parentQuery = parentQuery;
            }

            public List<Entry> Dependencies = new();
            public TimeSpan TotalTimeInSubQueries => _totalTimeInSubQueries;

            private IQuery _parentQuery;
            private readonly Db _db;
            private TimeSpan _totalTimeInSubQueries = TimeSpan.Zero;

            public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
            {
                
                var sw = Stopwatch.StartNew();
                var resultEntry = _db.Query(query, _parentQuery);
                Dependencies.Add(resultEntry);
                sw.Stop();
                _totalTimeInSubQueries = _totalTimeInSubQueries + sw.Elapsed;
                return (TQueryResult)(resultEntry.Result ?? throw new Exception("Query was not executed."));
            }
        }
    }
}
