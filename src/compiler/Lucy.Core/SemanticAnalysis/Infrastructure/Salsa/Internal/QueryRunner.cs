using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal;

public class QueryRunner
{
    private readonly Dictionary<Type, Func<object, object?>> _executors = new();

    public QueryRunner(QueryGroupInstanceProvider queryGroupInstanceProvider)
    {
        foreach (var queryGroupType in queryGroupInstanceProvider.KnownQueryGroupTypes)
        {
            var proxyType = queryGroupInstanceProvider.Get(queryGroupType).GetType();
            var methods = proxyType.GetMethods().Where(x => x.Name.StartsWith("UnwrapAndCall"));
            var instance = queryGroupInstanceProvider.Get(queryGroupType);

            foreach (var method in methods)
                _executors.Add(method.GetParameters()[0].ParameterType, CreateQueryExecutor(instance, method));
        }
    }

    private static Func<object, object> CreateQueryExecutor(object instance, MethodInfo method)
    {
        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var queryParameter = Expression.Parameter(typeof(object), "query");

        var convertedInstance = Expression.TypeAs(instanceParameter, method.DeclaringType ?? throw new Exception());
        var convertedQuery = Expression.TypeAs(queryParameter, method.GetParameters()[0].ParameterType);

        var call = Expression.TypeAs(Expression.Call(convertedInstance, method, convertedQuery), typeof(object));
        var lambda = Expression.Lambda(call, instanceParameter, queryParameter);
        var callableFunc = (Func<object, object, object>) lambda.Compile();

        return query => callableFunc(instance, query);
    }

    public object? Run(object query) => _executors[query.GetType()](query);
}