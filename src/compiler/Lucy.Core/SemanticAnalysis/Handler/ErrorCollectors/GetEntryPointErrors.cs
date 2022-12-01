using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public static class GetEntryPointErrorsHandler
{
    [GenerateDbExtension] ///<see cref="GetEntryPointErrorsEx.GetEntryPointErrors"/>
    public static ComparableReadOnlyList<Error> GetEntryPointErrors(IDb db)
    {
        var entryPoints = db.GetEntryPoints();
        var result = new ComparableReadOnlyList<Error>.Builder();

        if (entryPoints.Count == 0)
        {
            var documents = db.GetDocumentList();

            result.Add(new ErrorWithRange(documents.First(), new(new(0), new(0)), "No entry point found. Please ensure the solution has exactly one 'main' function."));
        }

        if (entryPoints.Count > 1)
        {
            foreach (var entryPoint in entryPoints)
            {
                var node = db.GetNodeById(entryPoint.NodeId);
                var nameNode = node.FunctionName.Token;

                result.Add(new ErrorWithRange(nameNode.NodeId.DocumentPath, db.GetRangeFromNode(nameNode), "More than one entry point was found. Please ensure the solution has only one 'main' function."));
            }
        }

        return result.Build();
    }
}