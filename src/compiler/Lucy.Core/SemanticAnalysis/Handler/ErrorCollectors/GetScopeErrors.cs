using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Collections.Generic;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public static class GetScopeErrorsHandler
{
    [DbQuery] ///<see cref="GetScopeErrorsEx.GetScopeErrors"/>
    public static ComparableReadOnlyList<Error> GetScopeErrors(IDb db, string documentPath)
    {
        var result = new List<Error>();

        foreach (var (usage, declarations) in db.GetSymbolMap(documentPath))
        {
            if (declarations.Count > 0)
                continue;

            var tokenNode = db.GetNodeById(usage);

            result.Add(new ErrorWithNodeId(usage, $"Could not resolve symbol '{tokenNode.Text}'."));
        }
        return result.ToComparableReadOnlyList();
    }
}