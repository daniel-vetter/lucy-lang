using System;
using System.Linq.Expressions;
using System.Reflection;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa.Internal.IlGeneration;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

public class RecordedCalculation
{
    public object Query { get; }
    public object? Result { get; }
    public TimeSpan ExecutionTime { get; }
    public ResultType ResultType { get; }

    public RecordedCalculation(object query, object? result, TimeSpan executionTime, ResultType resultType)
    {
        Query = query;
        Result = result;
        ExecutionTime = executionTime;
        ResultType = resultType;
    }

    public bool Is<T>(Expression<Func<T, Delegate>> expression)
    {
        if (expression.Body is not UnaryExpression {Operand: MethodCallExpression {Object: ConstantExpression {Value: MethodInfo expectedMethod}}})
            throw new Exception("Could not extract method from expression.");

        var expectedQueryGroupType = expectedMethod.DeclaringType ?? throw new Exception("Could not retrieve query group type from the expression.");

        var queryDataAttribute = Query.GetType().GetCustomAttribute<QueryDataAttribute>();
        if (queryDataAttribute == null)
            return false;

        if (queryDataAttribute.QueryGroupType != expectedQueryGroupType)
            return false;

        var allMembers = expectedQueryGroupType.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        return expectedMethod == allMembers[queryDataAttribute.MemberIndex];
    }

    public override string ToString()
    {
        return $"Recorded query: {Query.GetType().Name} -> {ResultType}: {Result}";
    }
}