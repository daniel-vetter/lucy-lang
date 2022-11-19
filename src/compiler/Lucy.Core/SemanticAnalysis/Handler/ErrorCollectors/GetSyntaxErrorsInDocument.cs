using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors
{
    public static class GetSyntaxErrors
    {
        [GenerateDbExtension] ///<see cref="GetAllSyntaxErrorsEx.GetAllSyntaxErrors"/>
        public static ComparableReadOnlyList<Error> GetAllSyntaxErrors(IDb db)
        {
            var result = new ComparableReadOnlyList<Error>.Builder();
            foreach(var documentPath in db.GetDocumentList())
            {
                result.AddRange(db.GetSyntaxErrorsInDocument(documentPath));
            }
            return result.Build();
        }

        [GenerateDbExtension] ///<see cref="GetSyntaxErrorsInDocumentEx.GetSyntaxErrorsInDocument"/>
        public static ComparableReadOnlyList<Error> GetSyntaxErrorsInDocument(IDb db, string documentPath)
        {
            var root = db.GetSyntaxTree(documentPath);
            var list = new ComparableReadOnlyList<Error>.Builder();
            Traverse(db, root, list);
            return list.Build();
        }

        [GenerateDbExtension] ///<see cref="GetSyntaxErrorsInStatementListEx.GetSyntaxErrorsInStatementList"/>
        public static ComparableReadOnlyList<Error> GetSyntaxErrorsInStatementList(IDb db, StatementListSyntaxNode statementListSyntaxNode)
        {
            var list = new ComparableReadOnlyList<Error>.Builder();
            Traverse(db, statementListSyntaxNode, list);
            return list.Build();
        }

        private static void Traverse(IDb db, SyntaxTreeNode node, ComparableReadOnlyList<Error>.Builder list)
        {
            list.AddRange(node.SyntaxErrors.Select(x => new Error(node, x)));

            foreach(var child in node.GetChildNodes())
            {
                if (child is StatementListSyntaxNode statementList)
                {
                    list.AddRange(db.GetSyntaxErrorsInStatementList(statementList));
                }
                else
                {
                    Traverse(db, child, list);
                }
            }
        }
    }
}
