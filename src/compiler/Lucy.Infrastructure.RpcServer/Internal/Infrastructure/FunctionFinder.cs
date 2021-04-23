using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Lucy.Infrastructure.RpcServer.Internal.Infrastructure
{
    internal class FunctionFinder
    {
        private readonly Dictionary<string, CallableFunction> _methods = new Dictionary<string, CallableFunction>();
        private JsonRpcConfig _config;

        public FunctionFinder(JsonRpcConfig config)
        {
            _config = config;

            foreach (var assembly in config.AssembliesToScan)
            {
                var types = assembly
                    .GetTypes()
                    .ToArray();

                foreach (var type in types)
                    FindFunctionsInClass(type);
            }
        }

        private void FindFunctionsInClass(Type type)
        {
            var duplicates = new HashSet<string>();
            var toAdd = new Dictionary<string, CallableFunction>();

            foreach (var methodInfo in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var parameterTypes = methodInfo.GetParameters().ToImmutableArray();
                var attribute = methodInfo.GetCustomAttribute<JsonRpcFunction>();
                if (attribute == null)
                    continue;

                var name = attribute.Name;

                if (_methods.ContainsKey(name) || toAdd.ContainsKey(name))
                {
                    duplicates.Add(name);
                    continue;
                }

                if (methodInfo.ReturnType == typeof(void))
                {
                    toAdd.Add(name, new CallableFunction(name, attribute.SingleParameter, parameterTypes, p =>
                    {
                        methodInfo.Invoke(_config.ControllerFactory(type), p);
                        return Task.FromResult<object?>(null);
                    }));
                }
                else if (methodInfo.ReturnType == typeof(Task))
                {
                    toAdd.Add(name, new CallableFunction(name, attribute.SingleParameter, parameterTypes, async p =>
                    {
                        await (methodInfo.Invoke(_config.ControllerFactory(type), p) as Task ?? throw new Exception("Function did not return a task."));
                        return null;
                    }));
                }
                else if (methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    toAdd.Add(name, new CallableFunction(name, attribute.SingleParameter, parameterTypes, async p =>
                    {
                        dynamic task = methodInfo.Invoke(_config.ControllerFactory(type), p) ?? throw new Exception("Function did not return a task.");
                        await task;
                        return task.Result;
                    }));
                }
                else
                {
                    toAdd.Add(name, new CallableFunction(name, attribute.SingleParameter, parameterTypes, p =>
                    {
                        var result = methodInfo.Invoke(_config.ControllerFactory(type), p);
                        return Task.FromResult(result);
                    }));
                }
            }

            if (duplicates.Count == 1)
                throw new Exception($"A function named '{duplicates.Single()}' was already registered.");
            if (duplicates.Count > 1)
                throw new Exception($"Functions named {string.Join(", ", duplicates.Select(x => $"'{x}'"))} were already registered.");

            foreach (var entry in toAdd)
                _methods.Add(entry.Key, entry.Value);
        }

        public CallableFunction? Find(string methodName)
        {
            if (_methods.TryGetValue(methodName, out var method))
                return method;

            return null;
        }
    }

    internal class CallableFunction
    {
        private readonly Func<object?[], Task<object?>> _handler;
        public string Name { get; }
        public bool SingleParameter { get; }
        public ImmutableArray<ParameterInfo> Parameters { get; }

        public CallableFunction(string name, bool singleParameter, ImmutableArray<ParameterInfo> parameter, Func<object?[], Task<object?>> handler)
        {
            Name = name;
            SingleParameter = singleParameter;
            _handler = handler;
            Parameters = parameter;
        }

        public async Task<object?> Invoke(object?[] parameters) => await _handler(parameters);
    }
}
