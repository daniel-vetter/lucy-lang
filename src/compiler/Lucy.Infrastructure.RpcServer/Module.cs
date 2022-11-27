using Lucy.Common.ServiceDiscovery;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Reflection;


namespace Lucy.Infrastructure.RpcServer;

public static class Module
{
    public static IServiceCollection AddJsonRpcServer(this IServiceCollection sc, Action<JsonRpcServerBuilder> build)
    {
        sc.AddServicesFromCurrentAssembly();
        var builder = new JsonRpcServerBuilder();
        build(builder);
        sc.AddSingleton(builder.CreateConfig());
        return sc;
    }
}

public class JsonRpcServerBuilder
{
    private readonly List<Assembly> _assembliesToScan = new();
    private readonly List<JsonConverter> _jsonConverter = new();
    private IPEndPoint? _networkEndpoint;

    public JsonRpcServerBuilder AddControllerFromCurrentAssembly()
    {
        _assembliesToScan.Add(Assembly.GetCallingAssembly());
        return this;
    }

    public JsonRpcServerBuilder AddJsonConverter<T>() where T : JsonConverter, new()
    {
        return AddJsonConverter(new T());
    }

    public JsonRpcServerBuilder AddJsonConverter(JsonConverter converter)
    {
        _jsonConverter.Add(converter);
        return this;
    }

    public JsonRpcServerBuilder ListenOnNetworkEndpoint(IPEndPoint networkEndpoint)
    {
        _networkEndpoint = networkEndpoint;
        return this;
    }

    internal JsonRpcConfig CreateConfig()
    {
        return new JsonRpcConfig(
            jsonConverter: _jsonConverter.ToImmutableArray(),
            assembliesToScan: _assembliesToScan.ToImmutableArray(),
            networkEndpoint: _networkEndpoint
        );
    }
}