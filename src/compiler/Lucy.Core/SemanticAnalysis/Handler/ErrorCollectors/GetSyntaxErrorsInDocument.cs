using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using System.Linq;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;

[QueryGroup]
public class GetSyntaxErrors
{
    private readonly Nodes _nodes;
    private readonly RangeResolver _rangeResolver;

    public GetSyntaxErrors(Nodes nodes, RangeResolver rangeResolver)
    {
        _nodes = nodes;
        _rangeResolver = rangeResolver;
    }

    public virtual ComparableReadOnlyList<Error> GetAllSyntaxErrors()
    {
        var result = new ComparableReadOnlyList<Error>.Builder();
        foreach(var documentPath in _nodes.GetDocumentList())
        {
            result.AddRange(GetSyntaxErrorsInDocument(documentPath));
        }
        return result.Build();
    }

    public virtual ComparableReadOnlyList<Error> GetSyntaxErrorsInDocument(string documentPath)
    {
        var root = _nodes.GetSyntaxTree(documentPath);
        var list = new ComparableReadOnlyList<Error>.Builder();
        Traverse(root, list);
        return list.Build();
    }

    public virtual ComparableReadOnlyList<Error> GetSyntaxErrorsInStatementList(INodeId<StatementListSyntaxNode> statementListSyntaxNodeId)
    {
        var list = new ComparableReadOnlyList<Error>.Builder();
        var statementListSyntaxNode = _nodes.GetNodeById(statementListSyntaxNodeId);
        Traverse(statementListSyntaxNode, list);
        return list.Build();
    }

    private void Traverse(SyntaxTreeNode node, ComparableReadOnlyList<Error>.Builder list)
    {
        if (!node.SyntaxErrors.IsDefaultOrEmpty)
            list.AddRange(node.SyntaxErrors.Select(x => new ErrorWithRange(node.NodeId.DocumentPath, _rangeResolver.GetTrimmedRangeFromNodeId(node.NodeId), x)));

        foreach(var child in node.GetChildNodes())
        {
            if (child is StatementListSyntaxNode statementList)
            {
                list.AddRange(GetSyntaxErrorsInStatementList(statementList.NodeId));
            }
            else
            {
                Traverse(child, list);
            }
        }
    }
}