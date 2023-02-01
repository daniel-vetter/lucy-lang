using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.Tests.CacheEngineTests;

public class QueryMetricsRecorder : IQueryListener
{
    private QueryMetrics? _last;

    public QueryMetrics GetLastQueryMetrics()
    {
        return _last ?? throw new Exception("No query executed.");
    }

    public void OnQueryExecuted(CacheEngineSnapshot snapshot)
    {
        _last = snapshot.LastQueryMetrics;
    }
}

public static class CacheEngineEx
{
    public static QueryMetrics GetLastQueryMetrics(this CacheEngine ce)
    {
        var recorder = ce.QueryStatisticListener as QueryMetricsRecorder;
        if (recorder == null)
            throw new Exception("Cache engine was not initialized with a query metrics recorder.");
        
        return recorder.GetLastQueryMetrics();
    }
}