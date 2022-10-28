using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis
{
    public class Db
    {
        private CallContext? _callContext = null;
        private Dictionary<IQuery, Entry> _entries = new();
        private Dictionary<Type, QueryHandler> _handlers = new();
        public Action<IDbEvent>? OnEvent { get; set; }
        private int _currentRevision;
        //TODO: Garbage collection

        private record CallContext(CallContext? ParentContext, IQuery Query, List<IQuery> Dependencies);

        public void RegisterHandler(QueryHandler handler)
        {
            _handlers.Add(handler.GetType().BaseType!.GetGenericArguments()[0], handler);
        }

        public void SetInput<TQueryResult>(IQuery<TQueryResult> query, TQueryResult result) where TQueryResult : notnull
        {
            _currentRevision++;
            if (_entries.TryGetValue(query, out var existingEntry))
            {
                if (!existingEntry.IsInput)
                    throw new Exception("The result of this query was already set by an query handle. It can not be changed to an input.");

                existingEntry.LastChanged = _currentRevision;
                existingEntry.Value = result;
            }
            else
            {
                _entries.Add(query, new Entry(result, _currentRevision, _currentRevision, true));
            }
            OnEvent?.Invoke(new InputWasChanged(query, result));
        }

        public void RemoveInput<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
        {
            if (!_entries.Remove(query))
                throw new Exception("The input could not be removed because it did not exist.");

            _currentRevision++;
            _entries.Remove(query);
            OnEvent?.Invoke(new InputWasRemoved(query));
        }

        private Entry EnsureEntryIsUpToDate(IQuery query)
        {
            if (!_entries.TryGetValue(query, out var entry))
                return Recalculate(query);

            if (entry.LastChecked == _currentRevision)
                return entry;

            foreach (var dependency in entry.Dependencies)
                EnsureEntryIsUpToDate(dependency);

            if (entry.Dependencies.Any(x => _entries[x].LastChanged > entry.LastChanged))
                Recalculate(query);

            entry.LastChecked = _currentRevision;
            return entry;
        }

        public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
        {
            OnEvent?.Invoke(new QueryReceived(query, _callContext?.Query));
            _callContext?.Dependencies.Add(query);
            var entry = EnsureEntryIsUpToDate(query);
            OnEvent?.Invoke(new QueryAnswered(query, _callContext?.Query));
            return (TQueryResult)entry.Value;
        }

        private Entry Recalculate(IQuery query)
        {
            if (!_handlers.TryGetValue(query.GetType(), out var handler))
                throw new Exception($"For a query of type '{query.GetType().Name}' is no input provided and no query handler registered.");

            OnEvent?.Invoke(new CalculationStarted(query));
            var stopwatch = Stopwatch.StartNew();
            var dependencies = new List<IQuery>();
            _callContext = new CallContext(_callContext, query, dependencies);
            object result;
            try
            {
                result = handler.Handle(this, query);
            }
            finally
            {
                _callContext = _callContext.ParentContext;
            }

            var resultWasSame = false;
            _entries.TryGetValue(query, out var entry);
            if (entry == null)
            {
                entry = new Entry(result, _currentRevision, _currentRevision, false, dependencies); ;
                _entries[query] = entry;
            }
            else
            {
                if (entry.IsInput)
                    throw new Exception("The result of this query was already set as an input. It can not be changed to an result of an query handler.");
                if (!result.Equals(entry.Value))
                {
                    resultWasSame = true;
                    entry.LastChanged = _currentRevision;
                    entry.Value = result;
                }
                entry.Dependencies = dependencies;
                entry.LastChecked = _currentRevision;
            }

            OnEvent?.Invoke(new CalculationFinished(query, entry.Value, stopwatch.Elapsed, resultWasSame));
            return entry;
        }

        private class Entry
        {
            public Entry(object value, int lastChanged, int lastChecked, bool isInput, List<IQuery>? dependencies = null)
            {
                Value = value;
                LastChanged = lastChanged;
                LastChecked = lastChecked;
                IsInput = isInput;
                Dependencies = dependencies ?? new List<IQuery>();
            }

            public int LastChanged { get; set; }
            public int LastChecked { get; set; }
            public bool IsInput { get; }
            public List<IQuery> Dependencies { get; set; }
            public object Value { get; set; }
        }
    }

    public abstract class QueryHandler
    {
        public abstract object Handle(Db runner, object query);
    }

    public abstract class QueryHandler<TQuery, TQueryResult> : QueryHandler where TQuery : notnull, IQuery<TQueryResult> where TQueryResult : notnull
    {
        public abstract TQueryResult Handle(Db runner, TQuery query);

        [DebuggerStepThrough]
        public override object Handle(Db db, object query)
        {
            return Handle(db, (TQuery)query);
        }
    }

    public interface IQuery { }
    public interface IQuery<TQueryResult> : IQuery { }

    public interface IDbEvent { }
    public record InputWasChanged(IQuery Query, object Value) : IDbEvent;
    public record InputWasRemoved(IQuery Query) : IDbEvent;
    public record CalculationStarted(IQuery Query) : IDbEvent;
    public record CalculationFinished(IQuery Query, object Result, TimeSpan Duration, bool ResultWasSame) : IDbEvent;
    public record QueryReceived(IQuery Query, IQuery? ParentQuery) : IDbEvent;
    public record QueryAnswered(IQuery Query, IQuery? ParentQuery) : IDbEvent;
}
