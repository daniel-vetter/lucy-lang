using System.Diagnostics;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

public class CacheEngine
{
    private readonly Worker _worker;
    public IQueryListener? QueryStatisticListener { get; }

    // TODO: Garbage collection
    // TODO: Multi Threading

    public CacheEngine(QueryGroupCollection queryGroupCollection, IQueryListener? queryStatisticListener = null)
    {
        queryGroupCollection.ThrowIfInvalid();

        QueryStatisticListener = queryStatisticListener;
        
        var db = new Db();
        var proxyTypeProvider = new QueryGroupProxyTypeProvider(queryGroupCollection);
        _worker = new Worker(db, proxyTypeProvider , queryStatisticListener);
    }

    [DebuggerStepThrough]
    public T Get<T>() => _worker.Get<T>();
}

public interface IQueryListener
{
    void OnQueryExecuted(CacheEngineSnapshot snapshot);
}

public enum ResultType
{
    InitialCalculation,
    WasTheSame,
    HasChanged
}