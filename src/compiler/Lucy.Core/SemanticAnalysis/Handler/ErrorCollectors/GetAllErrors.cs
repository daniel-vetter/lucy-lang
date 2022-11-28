using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public abstract record Error(string Message);
public record ErrorWithNode(SyntaxTreeNode Node, string Message) : Error(Message);
public record ErrorWithNodeId(NodeId NodeId, string Message) : Error(Message);
public record ErrorWithRange(string DocumentPath, Range1D Range, string Message) : Error(Message);

public static class GetErrors
{
    [GenerateDbExtension] ///<see cref="GetAllErrorsEx.GetAllErrors"/>
    public static ComparableReadOnlyList<ErrorWithRange> GetAllErrors(IDb db)
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        result.AddRange(db.GetImportErrors());
        result.AddRange(db.GetEntryPointErrors());
        result.AddRange(db.GetAllTypeErrors());

        foreach(var document in db.GetDocumentList())
        {
            result.AddRange(db.GetScopeErrors(document));
            result.AddRange(db.GetSyntaxErrorsInDocument(document));
        }
        return Remap(db, result.Build());
    }

    private static ComparableReadOnlyList<ErrorWithRange> Remap(IDb db, ComparableReadOnlyList<Error> errors)
    {
        var result = new ComparableReadOnlyList<ErrorWithRange>.Builder();
        foreach(var error in errors)
        {
            result.Add(error switch 
            {
                ErrorWithRange ewr => ewr,
                ErrorWithNode ewn => new ErrorWithRange(ewn.Node.NodeId.DocumentPath, db.GetRangeFromNode(ewn.Node), ewn.Message),
                ErrorWithNodeId ewni => new ErrorWithRange(ewni.NodeId.DocumentPath, db.GetRangeFromNode(db.GetNodeById(ewni.NodeId)), ewni.Message),
                _ => throw new NotSupportedException()
            });
        }
        return result.Build();
    }
}