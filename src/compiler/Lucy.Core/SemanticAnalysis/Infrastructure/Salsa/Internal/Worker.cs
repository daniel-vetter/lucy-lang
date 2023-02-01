using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal;

public class Worker
{
    private readonly Db _db;
    private readonly Stack<QueryContext> _queryContextStack = new();
    private readonly Stack<QueryContext> _queryContextPool = new();

    private readonly IQueryListener? _queryStatisticListener;
    private readonly List<RecordedCalculation> _recordedCalculations = new();

    private readonly QueryRunner _queryRunner;
    private readonly QueryGroupInstanceProvider _instanceProvider;

    public Worker(Db db, QueryGroupProxyTypeProvider queryGroupProxyTypeProvider, IQueryListener? queryStatisticListener)
    {
        _db = db;
        _instanceProvider = new QueryGroupInstanceProvider(queryGroupProxyTypeProvider, this);
        _queryRunner = new QueryRunner(_instanceProvider);
        _queryStatisticListener = queryStatisticListener;
    }

    private class QueryContext
    {
        public List<Entry> Dependencies { get; } = new();
        public Stopwatch Stopwatch { get; } = new();
    }

    public object? Query(object query)
    {
        _queryContextStack.TryPeek(out var parentQueryContext);

        if (_queryStatisticListener != null && parentQueryContext != null)
            parentQueryContext.Stopwatch.Stop();

        var resultEntry = GetUpToDateEntry(query);
        parentQueryContext?.Dependencies.Add(resultEntry);

        if (_queryStatisticListener != null)
        {
            if (parentQueryContext != null)
                parentQueryContext.Stopwatch.Start();
            else
            {
                _queryStatisticListener.OnQueryExecuted(CacheEngineSnapshot.Create(query, _recordedCalculations, _db.Entries));
                _recordedCalculations.Clear();
            }
        }

        return resultEntry.Result;
    }

    private void Recalculate(Entry entry)
    {
        if (!_queryContextPool.TryPop(out var queryContext))
            queryContext = new QueryContext();
        _queryContextStack.Push(queryContext);

        if (_queryStatisticListener != null)
            queryContext.Stopwatch.Start();
        var result = _queryRunner.Run(entry.Query);

        var elapsedExecutionTime = _queryStatisticListener != null ? queryContext.Stopwatch.Elapsed : TimeSpan.Zero;
        var dependencies = queryContext.Dependencies.ToArray();

        _queryContextStack.Pop();
        queryContext.Dependencies.Clear();
        if (_queryStatisticListener != null)
            queryContext.Stopwatch.Reset();
        _queryContextPool.Push(queryContext);
        var resultType = ResultType.WasTheSame;

        if (entry.LastChanged == 0)
        {
            resultType = ResultType.InitialCalculation;
            entry.LastChanged = _db.CurrentRevision;
            entry.Result = result;
        }
        else if (!IsEqual(result, entry.Result))
        {
            resultType = ResultType.HasChanged;
            entry.LastChanged = _db.CurrentRevision;
            entry.Result = result;
        }

        entry.Dependencies = dependencies;
        entry.LastChecked = _db.CurrentRevision;

        if (_queryStatisticListener != null)
            _recordedCalculations.Add(new RecordedCalculation(entry.Query, entry.Result, elapsedExecutionTime, resultType));
    }

    private Entry GetUpToDateEntry(object query)
    {
        if (!_db.Entries.TryGetValue(query, out var entry))
        {
            entry = new Entry(query, null, 0, 0, Array.Empty<Entry>());
            _db.Entries[query] = entry;
            Recalculate(entry);
        }
        else
            EnsureEntryIsUpToDate(entry);

        return entry;
    }

    private void EnsureEntryIsUpToDate(Entry entry)
    {
        if (entry.LastChecked == _db.CurrentRevision)
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

        entry.LastChecked = _db.CurrentRevision;
    }

    private bool IsEqual(object? result, object? entryResult)
    {
        if (result == null && entryResult != null) return false;
        if (result != null && entryResult == null) return false;
        if (result == null && entryResult == null) return true;
        if (result != null && entryResult != null) return result.Equals(entryResult);
        throw new NotSupportedException();
    }

    public void InvalidateEntry(object query)
    {
        _db.CurrentRevision++;

        var newValue = _queryRunner.Run(query);

        if (_db.Entries.TryGetValue(query, out var existingEntry))
        {
            existingEntry.LastChanged = _db.CurrentRevision;
            existingEntry.Result = newValue;
        }
        else
        {
            _db.Entries.Add(query, new Entry(query, newValue, _db.CurrentRevision, _db.CurrentRevision));
        }
    }

    [DebuggerStepThrough]
    public T Get<T>() => _instanceProvider.Get<T>();
}