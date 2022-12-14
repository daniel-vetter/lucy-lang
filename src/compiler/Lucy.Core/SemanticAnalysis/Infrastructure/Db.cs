using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Lucy.Common;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public class Db
{
    private readonly Dictionary<IQuery, Entry> _entries = new();
    private readonly Dictionary<Type, QueryHandler> _handlers = new();
    private int _currentRevision;

    private readonly Dictionary<Entry, RecordedCalculation> _lastQueryCalculations = new();
    private TimeSpan _lastQueryDuration = TimeSpan.Zero;
    private IQuery? _lastQuery;

    //TODO: Garbage collection

    public Action? OnQueryDone { get; set; }

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
            existingEntry.Result = result;
        }
        else
        {
            _entries.Add(query, new Entry(query, result, _currentRevision, _currentRevision, true));
        }
    }

    public void RemoveInput<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
    {
        if (!_entries.Remove(query, out var entry))
            throw new Exception("The input could not be removed because it did not exist.");

        _currentRevision++;
        entry.LastChanged = _currentRevision;
    }

    private bool EnsureEntryIsUpToDate(Entry entry)
    {
        if (entry.LastChecked == _currentRevision)
            return false;

        // First check the dependencies of the current entry.
        // If the current entry is out of date, we need to recalculate.
        foreach (var dep in entry.Dependencies)
        {
            if (dep.LastChanged > entry.LastChanged)
            {
                // Since we recalculated, the current node and all its dependencies
                // will be update to date, so we don't need to check further.
                return Recalculate(entry);
            }
        }

        // We now know that the current entry thinks it is up to date.
        // But transitive dependencies can still be out of date.

        foreach (var dep in entry.Dependencies)
        {
            // Recursively check for all dependencies, if there dependencies are up to date
            if (EnsureEntryIsUpToDate(dep))
            {
                // If something has changed, out current entry will also
                // no longer be up to date, so we need to recalculate.
                return Recalculate(entry);
            }
        }

        entry.LastChecked = _currentRevision;
        return false;
    }

    public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
    {
        
        _lastQueryCalculations.Clear();
        _lastQuery = query;
        var sw = Stopwatch.StartNew();
        var result = (TQueryResult)(Query((IQuery) query).Result ?? throw new Exception("Query was not executed."));
        _lastQueryDuration = sw.Elapsed;
        OnQueryDone?.Invoke();
        
        return result;
    }

    private Entry Query(IQuery query)
    {
        Profiler.Start("Query " + query.GetType().Name);
        if (!_entries.TryGetValue(query, out var entry))
        {
            entry = new Entry(query, null, 0, 0, false, new List<Entry>());
            _entries[query] = entry;
            Recalculate(entry);
        }

        EnsureEntryIsUpToDate(entry);
        Profiler.End("Query " + query.GetType().Name);
        return entry;
    }

    private bool Recalculate(Entry entry)
    {
        Profiler.Start("Calc " + entry.Query.GetType().Name);
        if (!_handlers.TryGetValue(entry.Query.GetType(), out var handler))
            throw new Exception($"For a query of type '{entry.Query.GetType().Name}' with parameter '{JsonSerializer.Serialize(entry.Query)}' is no input provided and no query handler registered.");

        var callContext = new QueryExecutionContext(this);
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

        _lastQueryCalculations.Add(entry, new RecordedCalculation(_lastQueryCalculations.Count, query: entry.Query, handlerStopwatch.Elapsed - callContext.TotalTimeInSubQueries, handlerStopwatch.Elapsed, overheadStopwatch.Elapsed, resultType));

        Profiler.End("Calc " + entry.Query.GetType().Name);
        return resultType != ResultType.WasTheSame;
    }

    public QueryExectionLog GetLastQueryExecutionLog()
    {
        if (_lastQuery == null)
            throw new Exception("No query was executed");

        if (!_entries.TryGetValue(_lastQuery, out var entry))
            throw new Exception("Query not found");

        var cache = new Dictionary<Entry, RecordedEntry>();

        RecordedEntry Map(Entry entryToMap)
        {
            if (cache.TryGetValue(entryToMap, out var alreadyMapped))
                return alreadyMapped;

            _lastQueryCalculations.TryGetValue(entryToMap, out var calculation);

            var mapped = new RecordedEntry(
                query: entryToMap.Query,
                result: entryToMap.Result,
                isInput: entryToMap.IsInput,
                dependencies: entryToMap.Dependencies.Select(Map).ToImmutableArray(),
                calculation: calculation
            );

            cache.Add(entryToMap, mapped);
            return mapped;
        }

        return new QueryExectionLog(_lastQueryDuration, Map(entry), _lastQueryCalculations.Values.ToImmutableArray());
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
        public QueryExecutionContext(Db db)
        {
            _db = db;
        }

        public List<Entry> Dependencies { get; } = new();
        public TimeSpan TotalTimeInSubQueries => _totalTimeInSubQueries;

        private readonly Db _db;
        private TimeSpan _totalTimeInSubQueries = TimeSpan.Zero;

        [DebuggerStepThrough]
        public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull
        {
            var sw = Stopwatch.StartNew();
            var resultEntry = _db.Query((IQuery) query);
            Dependencies.Add(resultEntry);
            sw.Stop();
            _totalTimeInSubQueries += sw.Elapsed;
            return (TQueryResult)(resultEntry.Result ?? throw new Exception("Query was not executed."));
        }
    }
}