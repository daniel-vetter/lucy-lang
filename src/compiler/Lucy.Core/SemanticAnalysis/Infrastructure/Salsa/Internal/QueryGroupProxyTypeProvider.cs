using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using System.Reflection.Emit;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal.IlGeneration;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal;

public class QueryGroupProxyTypeProvider
{
    private readonly Dictionary<Type, Type> _proxies = new();

    public QueryGroupProxyTypeProvider(QueryGroupCollection queryGroupCollection)
    {
        var moduleBuilder = AssemblyBuilder
            .DefineDynamicAssembly(new AssemblyName("SalsaDynamicAssembly"), AssemblyBuilderAccess.Run)
            .DefineDynamicModule("SalsaModule");

        foreach (var queryGroupType in queryGroupCollection.Types)
        {
            _proxies[queryGroupType] = ProxyBuilder.CreateProxyType(moduleBuilder, queryGroupType, queryGroupType.FullName + "Proxy");
        }
    }

    public Type GetProxyType(Type queryGroupType)
    {
        if (_proxies.TryGetValue(queryGroupType, out var proxyType))
            return proxyType;

        throw new Exception("No proxy was created for: " + queryGroupType);
    }

    public ImmutableArray<Type> KnownProxyTypes => _proxies.Values.ToImmutableArray();
}