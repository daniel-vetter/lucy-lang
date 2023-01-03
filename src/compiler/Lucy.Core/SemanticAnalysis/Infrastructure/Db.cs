using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using Lucy.Common;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public class Db
{
    private readonly Dictionary<object, Entry> _entries = new();
    private readonly Dictionary<object, Entry> _inputEntries = new();
    private readonly Dictionary<Type, QueryHandler> _handlers = new();
    private int _currentRevision;
    
    //TODO: Garbage collection
    //TODO: Better monitoring system so if no monitoring is requested, no performance impact is noticable
    
    public void RegisterHandler(QueryHandler handler)
    {
        _handlers.Add(handler.HandledType, handler);
    }

    public void SetInput(object query, object result)
    {
        if (_entries.ContainsKey(query))
            throw new Exception("The result of this query was already set by an query handle. It can not be changed to an input.");

        _currentRevision++;
        if (_inputEntries.TryGetValue(query, out var existingEntry))
        {
            existingEntry.LastChanged = _currentRevision;
            existingEntry.Result = result;
        }
        else
        {
            _inputEntries.Add(query, new Entry(query, result, _currentRevision, _currentRevision));
        }
    }

    public void RemoveInput(object query)
    {
        if (!_inputEntries.Remove(query, out var entry))
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

    public object? Query(object query)
    {
        return GetUpToDateEntry(query).Result;
    }

    private Entry GetUpToDateEntry(object query)
    {
        Profiler.Start("Query " + query.GetType().Name);
        if (!_entries.TryGetValue(query, out var entry) && !_inputEntries.TryGetValue(query, out entry))
        {
            entry = new Entry(query, null, 0, 0, Array.Empty<Entry>());
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
        //var handlerStopwatch = Stopwatch.StartNew();
        var result = handler.Handle(callContext, entry.Query);
        //handlerStopwatch.Stop();

        ResultType resultType;
       // var overheadStopwatch = Stopwatch.StartNew();

        if ((result == null && entry.Result != null) || (result != null && !result.Equals(entry.Result)))
        {
            resultType = ResultType.HasChanged;
            entry.LastChanged = _currentRevision;
            entry.Result = result;
        }
        else
        {
            resultType = ResultType.WasTheSame;
        }
        entry.Dependencies = callContext.Dependencies.ToArray();
        entry.LastChecked = _currentRevision;

        //overheadStopwatch.Stop();

        //_lastQueryCalculations.Add(entry, new RecordedCalculation(_lastQueryCalculations.Count, query: entry.Query, /*handlerStopwatch.Elapsed - callContext.TotalTimeInSubQueries*/ TimeSpan.Zero, TimeSpan.Zero, TimeSpan.Zero, /*handlerStopwatch.Elapsed, overheadStopwatch.Elapsed*/ resultType));

        Profiler.End("Calc " + entry.Query.GetType().Name);
        return resultType != ResultType.WasTheSame;
    }
    
    private class Entry
    {
        public Entry(object query, object? result, int lastChanged, int lastChecked, Entry[]? dependencies = null)
        {
            Query = query;
            Result = result;
            LastChanged = lastChanged;
            LastChecked = lastChecked;
            Dependencies = dependencies ?? Array.Empty<Entry>();
        }

        public int LastChanged { get; set; }
        public int LastChecked { get; set; }
        public Entry[] Dependencies { get; set; }
        public object Query { get; }
        public object? Result { get; set; }
    }

    private class QueryExecutionContext : IDb
    {
        public QueryExecutionContext(Db db)
        {
            _db = db;
        }

        public List<Entry> Dependencies { get; } = new();

        private readonly Db _db;
        
        [DebuggerStepThrough]
        public object? Query(object query)
        {
            return QueryInternal(query);
        }

        private object? QueryInternal(object query)
        {
            var resultEntry = _db.GetUpToDateEntry(query);
            Dependencies.Add(resultEntry);
            return resultEntry.Result;
        }
    }
}