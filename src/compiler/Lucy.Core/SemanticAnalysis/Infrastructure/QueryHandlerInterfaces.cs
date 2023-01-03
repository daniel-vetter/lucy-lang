using System;

namespace Lucy.Core.SemanticAnalysis.Infrastructure;

public abstract class QueryHandler
{
    public abstract object? Handle(IDb db, object query);
    public abstract Type HandledType { get; }
}

public interface IDb
{
    public object? Query(object query);
}

public interface IQuery { }
// ReSharper disable once UnusedTypeParameter
public interface IQuery<TQueryResult> : IQuery { }