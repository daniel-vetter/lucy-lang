using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lucy.Common.ServiceDiscovery;
using Newtonsoft.Json.Linq;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure;

[Service(Lifetime.Singleton)]
public class FunctionCaller
{
    private readonly JsonRpcSerializer _serializer;

    public FunctionCaller(JsonRpcSerializer serializer)
    {
        _serializer = serializer;
    }

    public async Task<FunctionCallResult> Call(CallableFunction function, JToken? parameter)
    {
        var parsedParameter = Parse(parameter, function);
        try
        {
            var result = await function.Invoke(parsedParameter);
            return new FunctionCallResult(null, result);
        }
        catch (Exception e)
        {
            return new FunctionCallResult(e, null);
        }
    }

    private object?[] Parse(JToken? json, CallableFunction function)
    {
        if (function.Parameters.Length == 0)
            return Array.Empty<object>();

        if (function.SingleParameter)
        {
            if (function.Parameters.Length != 1)
                throw new Exception("Single parameter deserialization was enabled but the method did not have excatly one parameter.");

            return ParseSingleParameter(json, function);
        }

        return ParseMultipleParameters(json, function);
    }

    private object?[] ParseMultipleParameters(JToken? json, CallableFunction function)
    {
        var result = new List<object?>();
        if (json is JObject obj)
        {
            foreach (var p in function.Parameters)
            {
                var name = p.Name ?? throw new Exception("missing parameter name");
                var prop = obj.Property(name);
                result.Add(prop == null ? null : _serializer.TokenToObject(prop.Value, p.ParameterType));
            }
        }

        if (json is JArray arr)
        {
            if (function.Parameters.Length < arr.Count)
                throw new Exception("Could parse function parameters because " + arr.Count + " parametere where provided, but the method only accepts " + function.Parameters.Length);

            for (int i = 0; i < arr.Count; i++)
            {
                result.Add(_serializer.TokenToObject(arr[i], function.Parameters[i].ParameterType));
            }
        }

        return result.ToArray();
    }

    private object?[] ParseSingleParameter(JToken? json, CallableFunction function)
    {
        if (json == null)
            return new object?[] { null };

        var result = _serializer.TokenToObject(json, function.Parameters[0].ParameterType);
        if (result == null)
            return new object?[] { null };

        return new[] { result };
    }
}

public record FunctionCallResult(Exception? Error, object? Result);