using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Reflection;

namespace Lucy.Infrastructure.RpcServer;

public class JsonRpcConfig
{
    public JsonRpcConfig(ImmutableArray<JsonConverter> jsonConverter, ImmutableArray<Assembly> assembliesToScan)
    {
        JsonConverter = jsonConverter;
        AssembliesToScan = assembliesToScan;
    }

    public IJsonRpcMessageTraceTarget? TraceTarget { get; private set; }
        
    public ImmutableArray<JsonConverter> JsonConverter { get; }
    public ImmutableArray<Assembly> AssembliesToScan { get; }

    public JsonRpcConfig TraceTo(IJsonRpcMessageTraceTarget? traceTarget)
    {
        TraceTarget = traceTarget;
        return this;
    }
}