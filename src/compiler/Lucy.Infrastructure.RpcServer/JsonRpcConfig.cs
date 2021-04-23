using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;

namespace Lucy.Infrastructure.RpcServer
{
    public class JsonRpcConfig
    {
        private List<Assembly> _assembliesToScan = new List<Assembly>();
        public ImmutableArray<Assembly> AssembliesToScan => _assembliesToScan.ToImmutableArray();
        public IJsonRpcMessageTrace? TraceTarget { get; private set; }
        public Func<Type, object> ControllerFactory { get; private set; } = x => Activator.CreateInstance(x) ?? throw new Exception("Could not create create controller of type: " + x.Name);

        public JsonRpcConfig AddControllersFrom(Assembly assembly)
        {
            _assembliesToScan.Add(assembly);
            return this;
        }

        public JsonRpcConfig AddControllersFromCurrentAssembly()
        {
            AddControllersFrom(Assembly.GetCallingAssembly());
            return this;
        }

        public JsonRpcConfig TraceTo(IJsonRpcMessageTrace? traceTarget)
        {
            TraceTarget = traceTarget;
            return this;
        }

        public JsonRpcConfig SetControllerFactory(Func<Type, object> createController)
        {
            ControllerFactory = createController;
            return this;
        }
    }
}
