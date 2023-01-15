using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Lucy.Common;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public class Db : IDb
{
    private readonly Dictionary<object, Entry> _entries = new();
    private readonly Dictionary<object, Entry> _inputEntries = new();
    private readonly Dictionary<Type, QueryHandler> _handlers = new();
    private int _currentRevision;
    
    private object? _lastRootQuery;
    private readonly List<RecordedQuery> _recordedCalculations = new();

    //TODO: Garbage collection

    public Db(bool trackQueryMetrics = false)
    {
        TrackQueryMetrics = trackQueryMetrics;
    }

    public bool TrackQueryMetrics { get; set; }

    public void RegisterHandler(QueryHandler handler)
    {
        _handlers.Add(handler.HandledType, handler);
    }

    public void RegisterHandlerFromCurrentAssembly()
    {
        foreach (var handler in Assembly.GetCallingAssembly().GetTypes().Where(x => x.BaseType == typeof(QueryHandler)))
        {
            var instance = Activator.CreateInstance(handler);
            if (instance == null)
                throw new Exception("Could not create instance of " + handler);

            RegisterHandler((QueryHandler)instance);
        }
    }

    public QueryMetrics GetLastQueryMetrics()
    {
        if (!TrackQueryMetrics)
            throw new Exception("TrackQueryMetrics was not enabled.");

        if (_lastRootQuery == null)
            throw new Exception("No query was executed.");

        return new QueryMetrics(_lastRootQuery, _recordedCalculations.ToImmutableArray());
    }

    public ImmutableArray<QueryTypeStatistic> GetQueryTypeStatistics()
    {
        return _entries
            .GroupBy(x => x.Key.GetType())
            .Select(x => new QueryTypeStatistic(x.Key, x.Count()))
            .ToImmutableArray();
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

    public object? Query(object query)
    {
        _recordedCalculations.Clear();
        _lastRootQuery = query;

        var result = GetUpToDateEntry(query).Result;
        return result;
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
        else
        {
            EnsureEntryIsUpToDate(entry);
        }
        
        Profiler.End("Query " + query.GetType().Name);
        return entry;
    }
    
    private void EnsureEntryIsUpToDate(Entry entry)
    {
        if (entry.LastChecked == _currentRevision)
            return;

        foreach (var dep in entry.Dependencies)
        {
            EnsureEntryIsUpToDate(dep);
            if (dep.LastChanged > entry.LastChecked)
            {
                Recalculate(entry);
                return;
            }
        }

        entry.LastChecked = _currentRevision;
    }

    private void Recalculate(Entry entry)
    {
        Profiler.Start("Calc " + entry.Query.GetType().Name);
        if (!_handlers.TryGetValue(entry.Query.GetType(), out var handler))
            throw new Exception($"For a query of type '{entry.Query.GetType().Name}' with parameter '{JsonSerializer.Serialize(entry.Query)}' is no input provided and no query handler registered.");

        var stopwatch = TrackQueryMetrics ? Stopwatch.StartNew() : null;

        var callContext = new QueryExecutionContext(this, TrackQueryMetrics);
        var result = handler.Handle(callContext, entry.Query);

        var resultType = ResultType.WasTheSame;

        if (entry.LastChanged == 0)
        {
            resultType = ResultType.InitialCalculation;
            entry.LastChanged = _currentRevision;
            entry.Result = result;
        }

        else if (!IsEqual(result, entry.Result))
        {
            resultType = ResultType.HasChanged;
            entry.LastChanged = _currentRevision;
            entry.Result = result;
        }
        entry.Dependencies = callContext.Dependencies.ToArray();
        entry.LastChecked = _currentRevision;

        if (TrackQueryMetrics)
        {
            var executionTime = stopwatch?.Elapsed - callContext.TimeInOtherQueries;
            if (executionTime == null)
                throw new Exception("Execution time could not be calculated");
            _recordedCalculations.Add(new RecordedQuery(entry.Query, entry.Result, executionTime.Value, resultType));
        }
        
        Profiler.End("Calc " + entry.Query.GetType().Name);
    }

    private bool IsEqual(object? result, object? entryResult)
    {
        if (result == null && entryResult != null) return false;
        if (result != null && entryResult == null) return false;
        if (result == null && entryResult == null) return true;
        if (result != null && entryResult != null) return result.Equals(entryResult);
        throw new NotSupportedException();
    }
    
    private class QueryExecutionContext : IDb
    {
        public QueryExecutionContext(Db db, bool measureTimeOnOtherQueries)
        {
            _db = db;
            if (measureTimeOnOtherQueries)
                _timeOnOtherQueries = new Stopwatch();
        }

        public List<Entry> Dependencies { get; } = new();

        private readonly Db _db;
        private readonly Stopwatch? _timeOnOtherQueries;

        public TimeSpan? TimeInOtherQueries => _timeOnOtherQueries?.Elapsed;

        [DebuggerStepThrough]
        public object? Query(object query)
        {
            _timeOnOtherQueries?.Start();
            var resultEntry = _db.GetUpToDateEntry(query);
            _timeOnOtherQueries?.Start();

            Dependencies.Add(resultEntry);
            return resultEntry.Result;
        }
    }
}

public class Entry
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


public class QueryMetrics
{
    public QueryMetrics(object rootQuery, ImmutableArray<RecordedQuery> calculations)
    {
        RootQuery = rootQuery;
        Calculations = calculations;
    }

    public object RootQuery { get; }
    public ImmutableArray<RecordedQuery> Calculations { get; }
}


public class RecordedQuery
{
    public object Query { get; }
    public object? Result { get; }
    public TimeSpan ExecutionTime { get; }
    public ResultType ResultType { get; }

    public RecordedQuery(object query, object? result, TimeSpan executionTime, ResultType resultType)
    {
        Query = query;
        Result = result;
        ExecutionTime = executionTime;
        ResultType = resultType;
    }
}

public class QueryTypeStatistic
{
    public QueryTypeStatistic(Type queryType, int count)
    {
        QueryType = queryType;
        Count = count;
    }

    public Type QueryType { get; }
    public int Count { get; }
}

public enum ResultType
{
    InitialCalculation,
    WasTheSame,
    HasChanged
}