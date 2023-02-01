using Lucy.Core.Parsing.Nodes;
using System.Linq;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

[QueryGroup]
public class GetEntryPointErrorsHandler
{
    private readonly EntryPointFinder _entryPointFinder;
    private readonly SemanticAnalysisInput _input;
    private readonly Nodes _nodes;

    public GetEntryPointErrorsHandler(EntryPointFinder entryPointFinder, SemanticAnalysisInput input, Nodes nodes)
    {
        _entryPointFinder = entryPointFinder;
        _input = input;
        _nodes = nodes;
    }
    
    public virtual ComparableReadOnlyList<Error> GetEntryPointErrors()
    {
        var entryPoints = _entryPointFinder.GetEntryPoints();
        var result = new ComparableReadOnlyList<Error>.Builder();

        if (entryPoints.Count == 0)
        {
            var documents = _nodes.GetDocumentList();

            var bestDocumentPath = documents.Select(x => new
                {
                    Path = x,
                    Depth = x.Count(ch => ch == '/')
                })
                .GroupBy(x => x.Depth)
                .MinBy(x => x.Key)?
                .MinBy(x => x.Path)?.Path;

            bestDocumentPath ??= documents.FirstOrDefault();

            if (bestDocumentPath != null)
            {
                result.Add(new ErrorWithRange(bestDocumentPath, new(0, 0), "No entry point found. Please ensure the solution has exactly one 'main' function."));
            }
        }

        if (entryPoints.Count > 1)
        {
            foreach (var entryPoint in entryPoints)
            {
                var node = _nodes.GetNodeById(entryPoint.NodeId);
                var nameNode = node.FunctionName;

                result.Add(new ErrorWithNodeId(nameNode.NodeId, "More than one entry point was found. Please ensure the solution has only one 'main' function."));
            }
        }

        return result.Build();
    }
}