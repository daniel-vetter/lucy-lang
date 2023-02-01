using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal.IlGeneration;

public static class ProxyBuilder
{
    public static Type CreateProxyType(ModuleBuilder moduleBuilder, Type queryGroupType, string name)
    {
        var tb = moduleBuilder.DefineType(
            name: name,
            attr: TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
            parent: queryGroupType
        );

        var dbField = tb.DefineField(
            fieldName: "_db",
            type: typeof(Worker),
            attributes: FieldAttributes.Private | FieldAttributes.InitOnly
        );

        CreateConstructor(tb, queryGroupType, dbField);

        var members = queryGroupType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        for (var i = 0; i < members.Length; i++)
        {
            var memberInfo = members[i];

            if (memberInfo.DeclaringType == typeof(object))
                continue;

            switch (memberInfo)
            {
                case MethodInfo {IsSpecialName: false, IsVirtual: true} m:
                {
                    var queryType = CreateQueryType(moduleBuilder, m, i);
                    CreateGetMethodOverride(tb, m, dbField, queryType);
                    CreateUnwrapAndCallMethod(tb, m, queryType);
                    break;
                }
                case PropertyInfo {IsSpecialName: false} p:
                {
                    var getMethod = p.GetGetMethod(true) ??
                                    throw new Exception("Could not resolve get method of property: " + p.Name);
                    if (getMethod.IsVirtual)
                    {
                        var queryType = CreateQueryType(moduleBuilder, getMethod, i);
                        CreateGetMethodOverride(tb, getMethod, dbField, queryType);
                        CreateUnwrapAndCallMethod(tb, getMethod, queryType);

                        var setMethod = p.GetSetMethod(true);
                        if (setMethod != null)
                            CreateSetMethodOverride(tb, setMethod, dbField, queryType);
                    }

                    break;
                }
            }
        }

        return tb.CreateType();
    }

    private static void CreateSetMethodOverride(TypeBuilder tb, MethodInfo methodToOverride, FieldBuilder dbField, Type queryType)
    {
        var m = tb.DefineMethod(methodToOverride.Name, MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual,
            methodToOverride.ReturnType, methodToOverride.GetParameters().Select(x => x.ParameterType).ToArray());
        var mil = m.GetILGenerator();

        // call the base setter
        mil.Emit(OpCodes.Ldarg_0);
        mil.Emit(OpCodes.Ldarg_1);
        mil.Emit(OpCodes.Call, methodToOverride);

        // create instance of the query object and call InvalidateEntry
        mil.Emit(OpCodes.Ldarg_0);
        mil.Emit(OpCodes.Ldfld, dbField);
        mil.Emit(OpCodes.Newobj, queryType.GetConstructors().Single());
        mil.Emit(OpCodes.Callvirt,
            typeof(Worker).GetMethod(nameof(Worker.InvalidateEntry)) ??
            throw new Exception("Could not find InvalidateEntry method on IDb"));

        mil.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(m, methodToOverride);
    }

    private static void CreateUnwrapAndCallMethod(TypeBuilder tb, MethodInfo methodToOverride, Type queryType)
    {
        var callM = tb.DefineMethod(
            name: $"UnwrapAndCall{methodToOverride.Name}",
            attributes: MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.HasThis,
            returnType: typeof(object),
            parameterTypes: new[] {queryType}
        );

        var callMIl = callM.GetILGenerator();
        callMIl.Emit(OpCodes.Ldarg_0);
        foreach (var parameterInfo in methodToOverride.GetParameters())
        {
            var name = ToUpperCamelCase(parameterInfo.Name ?? throw new Exception("Parameter has no name"));

            var getMethod = queryType
                .GetProperty(name)?
                .GetGetMethod() ?? throw new Exception($"Property {name} on {queryType} has no getter.");

            callMIl.Emit(OpCodes.Ldarg_1);
            callMIl.Emit(OpCodes.Callvirt, getMethod);
        }

        callMIl.Emit(OpCodes.Call, methodToOverride);
        if (methodToOverride.ReturnType.IsValueType)
            callMIl.Emit(OpCodes.Box, methodToOverride.ReturnType);
        callMIl.Emit(OpCodes.Ret);
    }

    private static string ToUpperCamelCase(string str) => str[..1].ToUpperInvariant() + str[1..];

    private static void CreateGetMethodOverride(TypeBuilder tb, MethodInfo methodToOverride, FieldBuilder dbField, Type queryType)
    {
        if (methodToOverride.IsGenericMethod)
            throw new NotSupportedException("Generic methods are not supported.");
        
        var m = tb.DefineMethod(methodToOverride.Name,
            MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Virtual,
            methodToOverride.ReturnType,
            methodToOverride.GetParameters().Select(x => x.ParameterType).ToArray());

        var mil = m.GetILGenerator();
        mil.Emit(OpCodes.Ldarg_0);
        mil.Emit(OpCodes.Ldfld, dbField);
        for (var i = 0; i < methodToOverride.GetParameters().Length; i++)
            mil.Emit(OpCodes.Ldarg, i + 1);
        mil.Emit(OpCodes.Newobj, queryType.GetConstructors().Single());
        mil.Emit(OpCodes.Callvirt,
            typeof(Worker).GetMethod(nameof(Worker.Query)) ?? throw new Exception("Could not find Query method on IDb"));
        mil.Emit(methodToOverride.ReturnType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass,
            methodToOverride.ReturnType);
        mil.Emit(OpCodes.Ret);
        tb.DefineMethodOverride(m, methodToOverride);
    }

    private static Type CreateQueryType(ModuleBuilder moduleBuilder, MethodInfo methodToOverride, int index)
    {
        var parameters = methodToOverride.GetParameters();

        var properties = parameters.Select(x =>
        {
            var name = ToUpperCamelCase(x.Name ?? throw new Exception("Property has no name"));
            return new RecordProperty(name, x.ParameterType);
        }).ToArray();

        var attributes = new[]
        {
            new RecordAttribute(typeof(QueryDataAttribute).GetConstructors().Single(x => x.GetParameters().Length == 2),
                new object[]
                {
                    methodToOverride.DeclaringType ?? throw new Exception("No declaring type found"),
                    index
                })
        };

        return RecordBuilder.Create(moduleBuilder, methodToOverride.Name + "Query_" + index, properties, attributes);
    }

    private static void CreateConstructor(TypeBuilder tb, Type type, FieldBuilder dbField)
    {
        var baseConstructor = type.GetConstructors().First();
        var constructorParameterTypes = new List<Type> {typeof(Worker)};
        constructorParameterTypes.AddRange(baseConstructor.GetParameters().Select(x => x.ParameterType));

        var ctr = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, constructorParameterTypes.ToArray());
        var ctrIl = ctr.GetILGenerator();

        // call base constructor
        ctrIl.Emit(OpCodes.Ldarg_0);
        for (var i = 1; i < constructorParameterTypes.Count; i++)
            ctrIl.Emit(OpCodes.Ldarg, i + 1);
        ctrIl.Emit(OpCodes.Call, baseConstructor);

        // set _db
        ctrIl.Emit(OpCodes.Ldarg_0);
        ctrIl.Emit(OpCodes.Ldarg_1);
        ctrIl.Emit(OpCodes.Stfld, dbField);
        ctrIl.Emit(OpCodes.Ret);
    }
}

public class QueryDataAttribute : Attribute
{
    public Type QueryGroupType { get; }
    public int MemberIndex { get; }

    public QueryDataAttribute(Type queryGroupType, int memberIndex)
    {
        QueryGroupType = queryGroupType;
        MemberIndex = memberIndex;
    }
}