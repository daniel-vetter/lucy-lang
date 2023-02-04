using Lucy.Core.Parsing.Nodes;
using System.Collections.Generic;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

[QueryGroup]
public class ScopeErrorsCollector
{
    private readonly SymbolResolver _symbolResolver;
    private readonly Nodes _nodes;

    public ScopeErrorsCollector(SymbolResolver symbolResolver, Nodes nodes)
    {
        _symbolResolver = symbolResolver;
        _nodes = nodes;
    }

    public virtual ComparableReadOnlyList<Error> GetAllScopeErrors()
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        foreach(var documentPath in _nodes.GetDocumentList())
        {
            result.AddRange(GetScopeErrors(documentPath));
        }
        return result.Build();
    }

    protected virtual ComparableReadOnlyList<Error> GetScopeErrors(string documentPath)
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