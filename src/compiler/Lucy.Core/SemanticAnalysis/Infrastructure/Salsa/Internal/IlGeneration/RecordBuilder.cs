using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal.IlGeneration;

public static class RecordBuilder
{
    public static Type Create(ModuleBuilder moduleBuilder, string name, RecordProperty[] properties, RecordAttribute[]? attributes = null)
    {
        var type = moduleBuilder.DefineType(name,TypeAttributes.Public | TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit);

        if (attributes != null)
            foreach (var attribute in attributes)
                type.SetCustomAttribute(new CustomAttributeBuilder(attribute.Constructor, attribute.Values));

        // Create auto properties with backing fields
        var fields = new List<FieldBuilder>();
        var createdProperties = new List<PropertyBuilder>();
        foreach (var recordProperty in properties)
        {
            var backingField = type.DefineField("<" + recordProperty.Name + ">BackingField", recordProperty.Type,
                FieldAttributes.Private | FieldAttributes.InitOnly);
            fields.Add(backingField);

            var property = type.DefineProperty(recordProperty.Name, PropertyAttributes.HasDefault, recordProperty.Type, null);

            var getter = type.DefineMethod("get_" + recordProperty.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                CallingConventions.HasThis, recordProperty.Type, Type.EmptyTypes);
            var getterIl = getter.GetILGenerator();
            getterIl.Emit(OpCodes.Ldarg_0);
            getterIl.Emit(OpCodes.Ldfld, backingField);
            getterIl.Emit(OpCodes.Ret);
            
            var setter = type.DefineMethod("set_" + recordProperty.Name, MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName,
                CallingConventions.HasThis, typeof(void), new[] {recordProperty.Type});

            var setterIl = setter.GetILGenerator();
            setterIl.Emit(OpCodes.Ldarg_0);
            setterIl.Emit(OpCodes.Ldarg_1);
            setterIl.Emit(OpCodes.Stfld, backingField);
            setterIl.Emit(OpCodes.Ret);

            property.SetGetMethod(getter);
            property.SetSetMethod(setter);
            createdProperties.Add(property);
        }

        // Create constructor
        var ctr = type.DefineConstructor(MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
            CallingConventions.Standard, properties.Select(x => x.Type).ToArray());
        for (var i = 0; i < properties.Length; i++)
            ctr.DefineParameter(i + 1, ParameterAttributes.None, properties[i].Name);

        var ctrIl = ctr.GetILGenerator();
        for (var i = 0; i < properties.Length; i++)
        {
            ctrIl.Emit(OpCodes.Ldarg_0);
            ctrIl.Emit(OpCodes.Ldarg_S, i + 1);
            ctrIl.Emit(OpCodes.Stfld, fields[i]);
        }

        // Call base class (object) constructor
        ctrIl.Emit(OpCodes.Ldarg_0);
        ctrIl.Emit(OpCodes.Call,
            typeof(object).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null) ??
            throw new Exception("Could not find object constructor"));
        ctrIl.Emit(OpCodes.Nop);
        ctrIl.Emit(OpCodes.Ret);

        // Equals
        var typedEquals = type.DefineMethod("Equals",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, CallingConventions.HasThis,
            typeof(bool), new Type[] {type});
        var typedEqualsIl = typedEquals.GetILGenerator();

        var returnTrue = typedEqualsIl.DefineLabel();
        var returnFalse = typedEqualsIl.DefineLabel();
        var end = typedEqualsIl.DefineLabel();

        // ReferenceEquals
        typedEqualsIl.Emit(OpCodes.Ldarg_0);
        typedEqualsIl.Emit(OpCodes.Ldarg_1);
        typedEqualsIl.Emit(OpCodes.Beq, returnTrue);

        // Check "other" for null
        typedEqualsIl.Emit(OpCodes.Ldarg_1);
        typedEqualsIl.Emit(OpCodes.Brfalse, returnFalse);

        for (var i = 0; i < fields.Count; i++)
        {
            var comparer = typeof(EqualityComparer<>).MakeGenericType(properties[i].Type);
            var getDefault = comparer.GetProperty("Default")?.GetGetMethod() ?? throw new Exception("Could not find compare method");
            typedEqualsIl.Emit(OpCodes.Call, getDefault);

            typedEqualsIl.Emit(OpCodes.Ldarg_0);
            typedEqualsIl.Emit(OpCodes.Ldfld, fields[i]);

            typedEqualsIl.Emit(OpCodes.Ldarg_1);
            typedEqualsIl.Emit(OpCodes.Ldfld, fields[i]);

            var comparerEqual = comparer.GetMethod("Equals", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly) ?? throw new Exception();
            typedEqualsIl.Emit(OpCodes.Callvirt, comparerEqual);
            typedEqualsIl.Emit(OpCodes.Brfalse_S, returnFalse);
        }

        typedEqualsIl.Emit(OpCodes.Br_S, returnTrue);

        typedEqualsIl.MarkLabel(returnFalse);
        typedEqualsIl.Emit(OpCodes.Ldc_I4_0);
        typedEqualsIl.Emit(OpCodes.Br_S, end);

        typedEqualsIl.MarkLabel(returnTrue);
        typedEqualsIl.Emit(OpCodes.Ldc_I4_1);

        typedEqualsIl.MarkLabel(end);
        typedEqualsIl.Emit(OpCodes.Ret);

        // Untyped equals
        var untypedEquals = type.DefineMethod("Equals",
            MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot, CallingConventions.HasThis,
            typeof(bool), new[] {typeof(object)});
        var untypedEqualsIl = untypedEquals.GetILGenerator();

        untypedEqualsIl.Emit(OpCodes.Ldarg_0);
        untypedEqualsIl.Emit(OpCodes.Ldarg_1);
        untypedEqualsIl.Emit(OpCodes.Isinst, type);
        untypedEqualsIl.Emit(OpCodes.Callvirt, typedEquals);
        untypedEqualsIl.Emit(OpCodes.Ret);

        var baseMethod = typeof(object).GetMethods().Single(x => x.Name == "Equals" && x.GetParameters().Length == 1);
        type.DefineMethodOverride(untypedEquals, baseMethod);

        //GetHashCode
        var getHashCode = type.DefineMethod("GetHashCode", MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
            CallingConventions.HasThis, typeof(int), Type.EmptyTypes);
        var getHashCodeIl = getHashCode.GetILGenerator();
        if (fields.Count > 0)
        {
            for (var i = 0; i < fields.Count; i++)
            {
                getHashCodeIl.Emit(OpCodes.Ldarg_0);
                getHashCodeIl.Emit(OpCodes.Call, createdProperties[i].GetGetMethod() ?? throw new Exception());
            }

            var combineMethod = typeof(HashCode).GetMethods().Single(x => x.Name == "Combine" && x.GetParameters().Length == fields.Count);
            combineMethod = combineMethod.MakeGenericMethod(properties.Select(x => x.Type).ToArray());
            getHashCodeIl.Emit(OpCodes.Call, combineMethod);
        }
        else
        {
            getHashCodeIl.Emit(OpCodes.Ldarg_0);
            getHashCodeIl.Emit(OpCodes.Call,
                typeof(object).GetMethod("GetType", BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Could not find GetType method"));
            getHashCodeIl.Emit(OpCodes.Callvirt,
                typeof(object).GetMethod("GetHashCode", BindingFlags.Public | BindingFlags.Instance) ?? throw new Exception("Could not find GetType method"));
        }

        getHashCodeIl.Emit(OpCodes.Ret);

        type.DefineMethodOverride(getHashCode, typeof(object).GetMethod("GetHashCode") ?? throw new Exception());

        return type.CreateType();
    }
}

public record RecordProperty(string Name, Type Type);
public record RecordAttribute(ConstructorInfo Constructor, object?[] Values);