using System.Diagnostics;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public abstract class QueryHandler
{
    public abstract object Handle(IDb db, object query);
}

public abstract class QueryHandler<TQuery, TQueryResult> : QueryHandler where TQuery : notnull, IQuery<TQueryResult> where TQueryResult : notnull
{
    public abstract TQueryResult Handle(IDb db, TQuery query);

    [DebuggerStepThrough]
    public override object Handle(IDb db, object query)
    {
        return Handle(db, (TQuery)query);
    }
}

public interface IDb
{
    public TQueryResult Query<TQueryResult>(IQuery<TQueryResult> query) where TQueryResult : notnull;
}

public interface IQuery { }
public interface IQuery<TQueryResult> : IQuery { }