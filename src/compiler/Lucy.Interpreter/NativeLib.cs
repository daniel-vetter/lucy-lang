using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Lucy.Interpreter
{
    public static class NativeLib
    {
        private static Dictionary<string, MethodInfo> _methods = new();

        public static object? Call(string libraryPath, string functionName, NativeType? returnType, params object?[]? arguments)
        {
            var types = (arguments ?? Array.Empty<object?>())
                .Select(x => ManagedToNativeType(x?.GetType()))
                .ToArray();

            var name = GetName(libraryPath, functionName, returnType, types);

            if (!_methods.TryGetValue(name, out var method))
            {
                method = CreateMethod(libraryPath, functionName, returnType, types);
                _methods.Add(name, method);
            }

            if (method == null)
                throw new Exception("Could not create pinvoke method.");

            return method.Invoke(null, arguments);

        }

        private static MethodInfo CreateMethod(string libraryName, string functionName, NativeType? returnType, IEnumerable<NativeType> parameterTypes)
        {
            var dynamicAsm = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("PInvokeAssembly"), AssemblyBuilderAccess.Run);
            var dynamicMod = dynamicAsm.DefineDynamicModule("PInvokeAssemblyModule");

            var typeBuilder = dynamicMod.DefineType("MyType", TypeAttributes.Public | TypeAttributes.UnicodeClass);

            var managedParameterTypes = parameterTypes
                .Select(NativeToManagedType)
                .ToArray();

            var managedReturnType = returnType == null ? null : NativeToManagedType(returnType.Value);

            var methodBuilder = typeBuilder.DefinePInvokeMethod(
                functionName,
                libraryName,
                functionName,
                MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.PinvokeImpl,
                CallingConventions.Standard,
                managedReturnType,
                managedParameterTypes,
                CallingConvention.Winapi,
                CharSet.Ansi);

            methodBuilder.SetImplementationFlags(methodBuilder.GetMethodImplementationFlags() | MethodImplAttributes.PreserveSig);

            var type = typeBuilder.CreateType() ?? throw new Exception("Could not type");
            var method = type.GetMethod(functionName);
            if (method == null)
                throw new Exception($"Function '{functionName}' was not declared.");
            return method;
        }

        private static string GetName(string libraryPath, string functionName, NativeType? returnType, IEnumerable<NativeType> parameters)
        {
            return $"{libraryPath}!{returnType?.ToString() ?? "void"} {functionName}({string.Join(", ", parameters)})";
        }

        private static Type NativeToManagedType(NativeType type)
        {
            if (type == NativeType.String)
                return typeof(string);
            else if (type == NativeType.Int32)
                return typeof(int);
            else if (type == NativeType.Ref)
                return typeof(string);
            else throw new Exception("Unsupported native type: " + type);
        }

        private static NativeType ManagedToNativeType(Type? type)
        {
            if (type == null)
                return NativeType.Ref;
            else if (type == typeof(string))
                return NativeType.String;
            else if (!type.IsValueType)
                return NativeType.Ref;
            else if (type == typeof(int))
                return NativeType.Int32;
            else throw new Exception($"Could not convert managed type {type} to a native type.");
        }
    }

    public enum NativeType
    {
        String,
        Int32,
        Ref
    }

}
