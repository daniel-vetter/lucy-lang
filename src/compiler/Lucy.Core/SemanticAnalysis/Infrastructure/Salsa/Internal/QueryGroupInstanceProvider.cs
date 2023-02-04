using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal;

public class QueryGroupInstanceProvider
{
    private readonly Dictionary<Type, object> _instances = new();
    private readonly Worker _worker;
    private readonly QueryGroupProxyTypeProvider _queryGroupProxyTypeProvider;

    public ImmutableArray<Type> KnownQueryGroupTypes { get; }

    public QueryGroupInstanceProvider(QueryGroupProxyTypeProvider queryGroupProxyTypeProvider, Worker worker)
    {
        _worker = worker;
        _queryGroupProxyTypeProvider = queryGroupProxyTypeProvider;

        foreach (var proxyType in queryGroupProxyTypeProvider.KnownProxyTypes)
            CreateInstance(proxyType);

        foreach (var instance in _instances.Values)
            CheckForAttributeInjects(instance);

        KnownQueryGroupTypes = _instances.Keys.ToImmutableArray();
    }

    private void CheckForAttributeInjects(object instance)
    {
        var fields = new List<FieldInfo>();
        var type = instance.GetType();
        while (type != null)
        {
            fields.AddRange(type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.GetCustomAttribute<InjectAttribute>() != null));
            type = type.BaseType;
        }

        foreach (var field in fields) 
            field.SetValue(instance, _instances[field.FieldType]);
    }

    private object CreateInstance(Type proxyType)
    {
        var queryGroupType = proxyType.BaseType ?? throw new Exception("Proxy has not base class.");

        if (_instances.TryGetValue(queryGroupType, out var existingInstance))
            return existingInstance;

        var ctrArgs = proxyType.GetConstructors()
            .Single()
            .GetParameters()
            .Select(x => x.ParameterType)
            .Select(x => x == typeof(Worker) ? _worker : CreateInstance(_queryGroupProxyTypeProvider.GetProxyType(x)))
            .ToArray();

        var proxyInstance = Activator.CreateInstance(proxyType, ctrArgs) ??
                            throw new Exception("Could not create the query group instance for: " + proxyType.Name);

        _instances[queryGroupType] = proxyInstance;
        return proxyInstance;
    }

    public T Get<T>() => (T) Get(typeof(T));
    public object Get(Type queryGroupType) => _instances[queryGroupType];
}