using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler;

[QueryGroup]
public class Functions
{
    private readonly Nodes _nodes;
    private readonly Flats _flats;
    private readonly SymbolResolver _symbolResolver;
    private readonly TypeResolver _typeResolver;

    public Functions(Nodes nodes, Flats flats, SymbolResolver symbolResolver, TypeResolver typeResolver)
    {
        _nodes = nodes;
        _flats = flats;
        _symbolResolver = symbolResolver;
        _typeResolver = typeResolver;
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
        var symbolDeclarations = _symbolResolver.GetSymbolDeclarations(flatFunctionCall.Name.NodeId);
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
                if (TypesMatch(flatCall.Arguments, flatDeclaration.Parameters))
                    return fd;
            }
        }
        return null;
    }

    private bool TypesMatch(ComparableReadOnlyList<INodeId<ExpressionSyntaxNode>> arguments, ComparableReadOnlyList<FlatFunctionDeclarationParameter> types)
    {
        if (arguments.Count != types.Count)
            return false;
        
        for (var i = 0; i < arguments.Count; i++)
        {
            var parameterTypeReference = types[i].TypeReference;
            if (parameterTypeReference == null)
                return false;

            var argument = arguments[i];
            var parameterType = _typeResolver.GetTypeInfo(parameterTypeReference);
            var argumentType = _typeResolver.GetTypeInfo(argument);

            if (parameterType == null || argumentType == null)
                return false;

            if (parameterType != argumentType)
                return false;
        }

        return true;
    }
}