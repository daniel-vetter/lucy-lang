﻿using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

[QueryGroup]
public class ImportErrorsCollector
{
    private readonly Imports _imports;
    private readonly RangeResolver _rangeResolver;
    private readonly Nodes _nodes;

    public ImportErrorsCollector(Imports imports, RangeResolver rangeResolver, Nodes nodes)
    {
        _imports = imports;
        _rangeResolver = rangeResolver;
        _nodes = nodes;
    }
    
    public virtual ComparableReadOnlyList<Error> GetImportErrors()
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        foreach (var documentPath in _nodes.GetDocumentList())
        {
            var imports = _imports.GetImports(documentPath);
            foreach (var import in imports.Invalid)
            {
                var range = _rangeResolver.GetTrimmedRangeFromNodeId(import.ImportPathTokenNodeId);
                result.Add(new ErrorWithRange(documentPath, range, $"Could not resolve import: '{import.Path}'."));
            }
        }
        return result.Build();
    }
}