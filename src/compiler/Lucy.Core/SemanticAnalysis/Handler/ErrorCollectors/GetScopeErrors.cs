using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Collections.Generic;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public static class GetScopeErrorsHandler
{
    [GenerateDbExtension] ///<see cref="GetScopeErrorsEx.GetScopeErrors"/>
    public static ComparableReadOnlyList<Error> GetScopeErrors(IDb db, string documentPath)
    {
        var result = new List<Error>();

        foreach(var functionCallNodeId in db.GetNodeIdsByType<FunctionCallExpressionSyntaxNode>(documentPath))
        {
            var candidates = db.GetFunctionCandidatesFromFunctionCall(functionCallNodeId);
            if (candidates.Count == 0)
            {
                var functionCallInfo = db.GetFunctionCallInfo(functionCallNodeId);
                result.Add(new ErrorWithNodeId(functionCallInfo.NameTokenNodeId, "Could not find a function called: " + functionCallInfo.Name));
            }
        }

        return result.ToComparableReadOnlyList();
    }
}