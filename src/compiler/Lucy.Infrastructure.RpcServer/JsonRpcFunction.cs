using JetBrains.Annotations;
using System;

namespace Lucy.Infrastructure.RpcServer;

[MeansImplicitUse]
public class JsonRpcFunction : Attribute
{
    public JsonRpcFunction(string functionName, bool deserializeParameterIntoSingleObject = true)
    {
        Name = functionName;
        SingleParameter = deserializeParameterIntoSingleObject;
    }

    public string Name { get; }
    public bool SingleParameter { get; }
}