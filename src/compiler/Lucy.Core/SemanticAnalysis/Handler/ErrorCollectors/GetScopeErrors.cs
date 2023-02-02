using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Collections.Generic;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

[QueryGroup]
public class GetScopeErrorsHandler
{
    private readonly SymbolResolver _symbolResolver;
    private readonly Nodes _nodes;

    public GetScopeErrorsHandler(SymbolResolver symbolResolver, Nodes nodes)
    {
        _symbolResolver = symbolResolver;
        _nodes = nodes;
    }
    
    public virtual ComparableReadOnlyList<Error> GetScopeErrors(string documentPath)
    {
        var result = new List<Error>();

        foreach (var (usage, declarations) in _symbolResolver.GetSymbolMap(documentPath))
        {
            if (declarations.Count > 0)
                continue;

            var tokenNode = _nodes.GetNodeById(usage);

            result.Add(new ErrorWithNodeId(usage, $"Could not resolve symbol '{tokenNode.Text}'."));
        }
        return result.ToComparableReadOnlyList();
    }
}