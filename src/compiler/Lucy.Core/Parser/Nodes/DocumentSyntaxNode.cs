using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Statements;
using Lucy.Core.Parser.Nodes.Trivia;

namespace Lucy.Core.Parser.Nodes
{
    public class DocumentSyntaxNode : SyntaxNode
    {
        public DocumentSyntaxNode(StatementListSyntaxNode statementList, TriviaListNode trailingTrivia)
        {
            StatementList = statementList;
            TrailingTrivia = trailingTrivia;
        }

        public StatementListSyntaxNode StatementList { get; }
        public TriviaListNode TrailingTrivia { get; }

        public static DocumentSyntaxNode ReadDocumentSyntaxNode(Code code)
        {
            var statementList = StatementListSyntaxNode.ReadStatementsWithoutBlock(code);
            var trailingTrivia = TriviaListNode.Read(code);

            return new DocumentSyntaxNode(statementList, trailingTrivia);
        }
    }
}
