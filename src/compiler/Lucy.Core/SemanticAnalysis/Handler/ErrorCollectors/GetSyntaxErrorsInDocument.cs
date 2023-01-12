using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

public static class GetSyntaxErrors
{
    [DbQuery] ///<see cref="GetAllSyntaxErrorsEx.GetAllSyntaxErrors"/>
    public static ComparableReadOnlyList<Error> GetAllSyntaxErrors(IDb db)
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        foreach(var documentPath in db.GetDocumentList())
        {
            result.AddRange(db.GetSyntaxErrorsInDocument(documentPath));
        }
        return result.Build();
    }

    [DbQuery] ///<see cref="GetSyntaxErrorsInDocumentEx.GetSyntaxErrorsInDocument"/>
    public static ComparableReadOnlyList<Error> GetSyntaxErrorsInDocument(IDb db, string documentPath)
    {
        var root = db.GetSyntaxTree(documentPath);
        var list = new ComparableReadOnlyList<Error>.Builder();
        Traverse(db, root, list);
        return list.Build();
    }

    [DbQuery] ///<see cref="GetSyntaxErrorsInStatementListEx.GetSyntaxErrorsInStatementList"/>
    public static ComparableReadOnlyList<Error> GetSyntaxErrorsInStatementList(IDb db, INodeId<StatementListSyntaxNode> statementListSyntaxNodeId)
    {
        var list = new ComparableReadOnlyList<Error>.Builder();
        var statementListSyntaxNode = db.GetNodeById(statementListSyntaxNodeId);
        Traverse(db, statementListSyntaxNode, list);
        return list.Build();
    }

    private static void Traverse(IDb db, SyntaxTreeNode node, ComparableReadOnlyList<Error>.Builder list)
    {
        if (!node.SyntaxErrors.IsDefaultOrEmpty)
            list.AddRange(node.SyntaxErrors.Select(x => new ErrorWithRange(node.NodeId.DocumentPath, db.GetRangeFromNodeId(node.NodeId), x)));

        foreach(var child in node.GetChildNodes())
        {
            if (child is StatementListSyntaxNode statementList)
            {
                list.AddRange(db.GetSyntaxErrorsInStatementList(statementList.NodeId));
            }
            else
            {
                Traverse(db, child, list);
            }
        }
    }
}