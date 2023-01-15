using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler;

public static class GetEntryPointsHandler
{
    [DbQuery] ///<see cref="GetEntryPointsEx.GetEntryPoints"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetEntryPoints(IDb db)
    {
        var paths = db.GetDocumentList();
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach (var path in paths)
        {
            var ids = db.GetEntryPointsInDocument(path);
            result.AddRange(ids);
        }
        return result.Build();
    }

    [DbQuery] ///<see cref="GetEntryPointsInDocumentEx.GetEntryPointsInDocument"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetEntryPointsInDocument(IDb db, string documentPath)
    {
        var infos = db.GetTopLevelFunctions(documentPath);
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach(var info in infos)
        {
            if (info.Name.Text == "main")
                result.Add(info);
        }
        return result.Build();
    }
}