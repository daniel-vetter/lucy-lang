using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using System;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public abstract record Error(string Message);
public record ErrorWithNodeId(INodeId<SyntaxTreeNode> NodeId, string Message) : Error(Message);
public record ErrorWithRange(string DocumentPath, Range1D Range, string Message) : Error(Message);

[QueryGroup]
public class AllErrorsCollector
{
    private readonly ImportErrorsCollector _importErrorsCollector;
    private readonly EntryPointErrorsCollector _entryPointErrorsCollector;
    private readonly TypeErrorsCollector _typeErrorsCollector;
    private readonly SyntaxErrorsCollector _syntaxErrorsCollector;
    private readonly ScopeErrorsCollector _scopeErrorsCollector;
    private readonly RangeResolver _rangeResolver;
    private readonly FunctionCallErrorCollector _functionCallErrorCollector;

    public AllErrorsCollector(
        ImportErrorsCollector importErrorsCollector, 
        EntryPointErrorsCollector entryPointErrorsCollector, 
        TypeErrorsCollector typeErrorsCollector,
        SyntaxErrorsCollector syntaxErrorsCollector,
        ScopeErrorsCollector scopeErrorsCollector,
        RangeResolver rangeResolver,
        FunctionCallErrorCollector functionCallErrorCollector)
    {
        _importErrorsCollector = importErrorsCollector;
        _entryPointErrorsCollector = entryPointErrorsCollector;
        _typeErrorsCollector = typeErrorsCollector;
        _syntaxErrorsCollector = syntaxErrorsCollector;
        _scopeErrorsCollector = scopeErrorsCollector;
        _rangeResolver = rangeResolver;
        _functionCallErrorCollector = functionCallErrorCollector;
    }

    public virtual ComparableReadOnlyList<ErrorWithRange> GetAllErrors()
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        result.AddRange(_importErrorsCollector.GetImportErrors());
        result.AddRange(_entryPointErrorsCollector.GetEntryPointErrors());
        result.AddRange(_typeErrorsCollector.GetAllTypeErrors());
        result.AddRange(_syntaxErrorsCollector.GetAllSyntaxErrors());
        result.AddRange(_scopeErrorsCollector.GetAllScopeErrors());
        result.AddRange(_functionCallErrorCollector.GetAllFunctionCallErrors());
        return Remap(result.Build());
    }

    private ComparableReadOnlyList<ErrorWithRange> Remap(ComparableReadOnlyList<Error> errors)
    {
        var result = new ComparableReadOnlyList<ErrorWithRange>.Builder();
        foreach (var error in errors)
        {
            result.Add(error switch
            {
                ErrorWithRange errorWithRange => errorWithRange,
                ErrorWithNodeId errorWithNodeId => new ErrorWithRange(errorWithNodeId.NodeId.DocumentPath, _rangeResolver.GetTrimmedRangeFromNodeId(errorWithNodeId.NodeId), errorWithNodeId.Message),
                _ => throw new NotSupportedException()
            });
        }
        return result.Build();
    }
}