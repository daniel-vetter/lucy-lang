using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public static class GetScopeErrorsHandler
{
    [DbQuery] ///<see cref="GetScopeErrorsEx.GetScopeErrors"/>
    public static ComparableReadOnlyList<Error> GetScopeErrors(IDb db, string documentPath)
    {
        var result = new List<Error>();
        
        foreach (var functionCallNodeId in db.GetNodeIdsByType<FunctionCallExpressionSyntaxNode>(documentPath))
        {
            var best = db.GetBestMatchingFunctionsFromFunctionCall(functionCallNodeId);
            if (best == null)
            {
                var candidates = db.GetFunctionCandidatesFromFunctionCall(functionCallNodeId);
                if (candidates.Count == 0)
                {
                    var functionCallInfo = db.GetFlatFunctionCall(functionCallNodeId);
                    result.Add(new ErrorWithNodeId(functionCallInfo.Name.NodeId, "Could not find a function called: " + functionCallInfo.Name.Text));
                }

                if (candidates.Count == 1)
                {
                    var functionCallInfo = db.GetFlatFunctionCall(functionCallNodeId);
                    result.Add(new ErrorWithNodeId(functionCallInfo.Name.NodeId, "Can not call function " + functionCallInfo.Name.Text + " because the provided arguments do not match the defined signature."));
                }

                if (candidates.Count > 1)
                {
                    var functionCallInfo = db.GetFlatFunctionCall(functionCallNodeId);
                    result.Add(new ErrorWithNodeId(functionCallInfo.Name.NodeId, "Can not call function " + functionCallInfo.Name.Text + " because multiple matching overloads have been found."));
                }
            }
        }

        return result.ToComparableReadOnlyList();
    }
}