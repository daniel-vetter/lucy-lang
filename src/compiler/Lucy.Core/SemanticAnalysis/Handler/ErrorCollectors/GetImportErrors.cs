﻿using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public static class GetImportErrorsHandler
{
    [GenerateDbExtension] ///<see cref="GetImportErrorsEx.GetImportErrors"/>
    public static ComparableReadOnlyList<Error> GetImportErrors(IDb db)
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        foreach (var documentPath in db.GetDocumentList())
        {
            var imports = db.GetImports(documentPath);
            foreach (var import in imports)
            {
                if (import.ValidState == ImportValidationResult.Ok)
                    continue;

                var range = db.GetRangeFromNode(db.GetNodeById(import.NodeId));
                
                if (import.ValidState == ImportValidationResult.CouldNotResolve)
                {
                    result.Add(new ErrorWithRange(documentPath, range, $"Could not resolve import: '{import.Path}'."));
                }
            }
        }
        return result.Build();
    }
}