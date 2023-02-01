using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler;

[QueryGroup]
public class Functions
{
    private readonly Nodes _nodes;
    private readonly Flats _flats;
    private readonly Symbols _symbols;
    private readonly Types _types;

    public Functions(Nodes nodes, Flats flats, Symbols symbols, Types types)
    {
        _nodes = nodes;
        _flats = flats;
        _symbols = symbols;
        _types = types;
    }
    
    /// <summary>
    /// Returns a list of all top level function declarations in a document.
    /// </summary>
    public virtual ComparableReadOnlyList<FlatFunctionDeclaration> GetTopLevelFunctions(string documentPath)
    {
        var root = _nodes.GetSyntaxTree(documentPath);
        var topLevelFunctions = _nodes.GetNodeIdsByTypeInStatementListShallow<FunctionDeclarationStatementSyntaxNode>(root.StatementList.NodeId);
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach (var topLevelFunction in topLevelFunctions)
        {
            result.Add(_flats.GetFlatFunctionDeclaration(topLevelFunction));
        }
        return result.Build();
    }

    /// <summary>
    /// Returns all matching function call targets.
    ///  1) Name must match
    ///  2) Function declaration needs to be in scope of the function call
    /// </summary>
    public virtual ComparableReadOnlyList<INodeId<SyntaxTreeNode>> GetAllPossibleFunctionCallTargets(INodeId<FunctionCallExpressionSyntaxNode> nodeId)
    {
        var flatFunctionCall = _flats.GetFlatFunctionCall(nodeId);
        var symbolDeclarations = _symbols.GetSymbolDeclarations(flatFunctionCall.Name.NodeId);
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
    public virtual INodeId<SyntaxTreeNode>? GetBestFunctionCallTarget(INodeId<FunctionCallExpressionSyntaxNode> nodeId)
    {
        var flatCall = _flats.GetFlatFunctionCall(nodeId);
        var all = GetAllPossibleFunctionCallTargets(nodeId);
        foreach (var functionCallTarget in all)
        {
            if (functionCallTarget is INodeId<FunctionDeclarationStatementSyntaxNode> fd)
            {
                var flatDeclaration = _flats.GetFlatFunctionDeclaration(fd);
                if (flatDeclaration.Parameters.Count != flatCall.Arguments.Count)
                    continue;
                
                for (var i = 0; i < flatCall.Arguments.Count; i++)
                {
                    var parameterTypeReference = flatDeclaration.Parameters[i].TypeReference;
                    if (parameterTypeReference == null)
                        break;

                    var argument = flatCall.Arguments[i];
                    var parameterType = _types.GetTypeInfoFromTypeReferenceId(parameterTypeReference);
                    var argumentType = _types.GetTypeInfoFromExpression(argument);

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