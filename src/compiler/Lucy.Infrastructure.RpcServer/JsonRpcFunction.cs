using System;

namespace Lucy.Infrastructure.RpcServer
{
    public class JsonRpcFunction : Attribute
    {
        public JsonRpcFunction(string functionName, bool deserializeParamterIntoSingleObject = true)
        {
            Name = functionName;
            SingleParameter = deserializeParamterIntoSingleObject;
        }

        public string Name { get; }
        public bool SingleParameter { get; }
    }
}
