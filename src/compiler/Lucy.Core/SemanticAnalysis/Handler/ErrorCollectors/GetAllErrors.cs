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
public class GetErrors
{
    private readonly GetImportErrorsHandler _getImportErrorsHandler;
    private readonly GetEntryPointErrorsHandler _getEntryPointErrorsHandler;
    private readonly GetTypeErrors _getTypeErrors;
    private readonly GetSyntaxErrors _getSyntaxErrors;
    private readonly GetScopeErrorsHandler _getScopeErrorsHandler;
    private readonly Ranges _ranges;
    private readonly Nodes _nodes;

    public GetErrors(GetImportErrorsHandler getImportErrorsHandler, GetEntryPointErrorsHandler getEntryPointErrorsHandler, GetTypeErrors getTypeErrors, GetSyntaxErrors getSyntaxErrors, GetScopeErrorsHandler getScopeErrorsHandler, Ranges ranges, Nodes nodes)
    {
        _getImportErrorsHandler = getImportErrorsHandler;
        _getEntryPointErrorsHandler = getEntryPointErrorsHandler;
        _getTypeErrors = getTypeErrors;
        _getSyntaxErrors = getSyntaxErrors;
        _getScopeErrorsHandler = getScopeErrorsHandler;
        _ranges = ranges;
        _nodes = nodes;
    }

    public virtual ComparableReadOnlyList<ErrorWithRange> GetAllErrors()
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        result.AddRange(_getImportErrorsHandler.GetImportErrors());
        result.AddRange(_getEntryPointErrorsHandler.GetEntryPointErrors());
        result.AddRange(_getTypeErrors.GetAllTypeErrors());

        foreach (var document in _nodes.GetDocumentList())
        {
            result.AddRange(_getScopeErrorsHandler.GetScopeErrors(document));
            result.AddRange(_getSyntaxErrors.GetSyntaxErrorsInDocument(document));
        }

        return Remap(result.Build());
    }

    private ComparableReadOnlyList<ErrorWithRange> Remap(ComparableReadOnlyList<Error> errors)
    {
        var result = new ComparableReadOnlyList<ErrorWithRange>.Builder();
        foreach (var error in errors)
        {
            result.Add(error switch
            {
                ErrorWithRange ewr => ewr,
                ErrorWithNodeId ewni => new ErrorWithRange(ewni.NodeId.DocumentPath, _ranges.GetRangeFromNodeId(ewni.NodeId), ewni.Message),
                _ => throw new NotSupportedException()
            });
        }

        return result.Build();
    }
}