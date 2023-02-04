using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

[QueryGroup]
public class FunctionCallErrorCollector
{
    private readonly Nodes _nodes;
    private readonly Functions _functions;
    private readonly Flats _flats;

    public FunctionCallErrorCollector(Nodes nodes, Functions functions, Flats flats)
    {
        _nodes = nodes;
        _functions = functions;
        _flats = flats;
    }
    
    public virtual ComparableReadOnlyList<Error> GetAllFunctionCallErrors()
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        foreach (var documentPath in _nodes.GetDocumentList())
        {
            result.AddRange(GetFunctionCallErrors(documentPath));
        }
        return result.Build();
    }

    protected virtual ComparableReadOnlyList<Error> GetFunctionCallErrors(string documentPath)
    {
        var functionCalls = _nodes.GetNodeIdsByType<FunctionCallExpressionSyntaxNode>(documentPath);
        var result = new ComparableReadOnlyList<Error>.Builder();
        foreach (var functionCall in functionCalls)
        {
            if (_functions.GetBestFunctionCallTarget(functionCall) == null)
            {
                var flat = _flats.GetFlatFunctionCall(functionCall);
                result.Add(new ErrorWithNodeId(flat.Name.NodeId, $"Could not find a function called '{flat.Name.Text}' with the matching parameters."));
            }
        }
        return result.Build();
    }
}