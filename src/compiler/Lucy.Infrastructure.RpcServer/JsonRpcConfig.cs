using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Net;
using System.Reflection;

namespace Lucy.Infrastructure.RpcServer;

public class JsonRpcConfig
{
    public JsonRpcConfig(ImmutableArray<JsonConverter> jsonConverter, ImmutableArray<Assembly> assembliesToScan, IPEndPoint? networkEndpoint)
    {
        JsonConverter = jsonConverter;
        AssembliesToScan = assembliesToScan;
        NetworkEndpoint = networkEndpoint;
    }

    public IJsonRpcMessageTraceTarget? TraceTarget { get; private set; }
        
    public ImmutableArray<JsonConverter> JsonConverter { get; }
    public ImmutableArray<Assembly> AssembliesToScan { get; }
    public IPEndPoint? NetworkEndpoint { get; }

    public JsonRpcConfig TraceTo(IJsonRpcMessageTraceTarget? traceTarget)
    {
        TraceTarget = traceTarget;
        return this;
    }
}