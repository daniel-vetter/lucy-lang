using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler;

public static class FunctionsHandler
{
    /// <summary>
    /// Returns a list of all top level function declarations in a document.
    /// </summary>
    /// <see cref="GetTopLevelFunctionsEx.GetTopLevelFunctions"/>
    /// <returns></returns>
    [DbQuery]
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetTopLevelFunctions(IDb db, string documentPath)
    {
        var root = db.GetSyntaxTree(documentPath);
        var topLevelFunctions = db.GetNodeIdsByTypeInStatementListShallow<FunctionDeclarationStatementSyntaxNode>(root.StatementList.NodeId);
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach (var topLevelFunction in topLevelFunctions)
        {
            result.Add(db.GetFlatFunctionDeclaration(topLevelFunction));
        }
        return result.Build();
    }

    /// <summary>
    /// Returns all matching function call targets.
    ///  1) Name must match
    ///  2) Function declaration needs to be in scope of the function call
    /// </summary>
    /// <see cref="GetAllPossibleFunctionCallTargetsEx.GetAllPossibleFunctionCallTargets"/>
    [DbQuery]
    public static ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetAllPossibleFunctionCallTargets(IDb db, INodeId<FunctionCallExpressionSyntaxNode> nodeId)
    {
        var flatFunctionCall = db.GetFlatFunctionCall(nodeId);
        var symbolDeclarations = db.GetSymbolDeclarations(flatFunctionCall.Name.NodeId);
        var result = new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>.Builder();

        foreach (var symbolDeclaration in symbolDeclarations)
        {
            if (symbolDeclaration.DeclaringNodeId is INodeId<FunctionDeclarationStatementSyntaxNode> fd)
                result.Add(fd);
        }

        return result.Build();
    }

    /// <summary>
    /// Returns the best matching function call target.
    ///  1) Name must match
    ///  2) Function declaration needs to be in scope of the function call
    ///  3) Function argument count needs to match the function parameter count
    ///  4) Argument types need to match
    /// </summary>
    /// <see cref="GetBestFunctionCallTargetEx.GetBestFunctionCallTarget"/>
    [DbQuery]
    public static INodeId<SyntaxTreeNode>? GetBestFunctionCallTarget(IDb db, INodeId<FunctionCallExpressionSyntaxNode> nodeId)
    {
        var flatCall = db.GetFlatFunctionCall(nodeId);
        var all = db.GetAllPossibleFunctionCallTargets(nodeId);
        foreach (var functionCallTarget in all)
        {
            if (functionCallTarget is INodeId<FunctionDeclarationStatementSyntaxNode> fd)
            {
                var flatDeclaration = db.GetFlatFunctionDeclaration(fd);
                if (flatDeclaration.Parameters.Count != flatCall.Arguments.Count)
                    continue;
                
                for (var i = 0; i < flatCall.Arguments.Count; i++)
                {
                    var parameterTypeReference = flatDeclaration.Parameters[i].TypeReference;
                    if (parameterTypeReference == null)
                        break;

                    var argument = flatCall.Arguments[i];
                    var parameterType = db.GetTypeInfoFromTypeReferenceId(parameterTypeReference);
                    var argumentType = db.GetTypeInfoFromExpression(argument);

                    if (parameterType == null || argumentType == null)
                        break;

                    if (parameterType != argumentType)
                        break;

                    return fd;
                }
            }
        }
        return null;
    }
}