using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

public class CacheEngineSnapshot
{
    public CacheEngineSnapshot(QueryMetrics? lastQueryMetrics, ImmutableDictionary<Type, int> cachedQueriesByType)
    {
        LastQueryMetrics = lastQueryMetrics;
        CachedQueriesByType = cachedQueriesByType;
    }

    public static CacheEngineSnapshot Create(object? lastRootQuery, IEnumerable<RecordedCalculation> recordedQueries, IDictionary<object, Entry> entries)
    {
        return new CacheEngineSnapshot(
            lastRootQuery != null ? new QueryMetrics(lastRootQuery, recordedQueries.ToImmutableArray()) : null,
            entries.GroupBy(x => x.Key.GetType()).ToImmutableDictionary(x => x.Key, x => x.Count())
        );
    }

    public QueryMetrics? LastQueryMetrics { get; }
    public ImmutableDictionary<Type, int> CachedQueriesByType { get; }
}

public class QueryMetrics
{
    public QueryMetrics(object rootQuery, ImmutableArray<RecordedCalculation> calculations)
    {
        RootQuery = rootQuery;
        Calculations = calculations;
    }

    public object RootQuery { get; }
    public ImmutableArray<RecordedCalculation> Calculations { get; }
}