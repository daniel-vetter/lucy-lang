using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler;

[QueryGroup]
public class EntryPointFinder
{
    private readonly Nodes _nodes;
    private readonly Functions _functions;

    public EntryPointFinder(Nodes nodes, Functions functions)
    {
        _nodes = nodes;
        _functions = functions;
    }
    
    public virtual ComparableReadOnlyList<FlatFunctionDeclaration> GetEntryPoints()
    {
        var paths = _nodes.GetDocumentList();
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach (var path in paths)
        {
            var ids = GetEntryPointsInDocument(path);
            result.AddRange(ids);
        }
        return result.Build();
    }

    public virtual ComparableReadOnlyList<FlatFunctionDeclaration> GetEntryPointsInDocument(string documentPath)
    {
        var infos = _functions.GetTopLevelFunctions(documentPath);
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach(var info in infos)
        {
            if (info.Name.Text == "main")
                result.Add(info);
        }
        return result.Build();
    }
}