using Lucy.Core.Parsing.Nodes.Statements;
using Lucy.Core.Parsing.Nodes.Trivia;

namespace Lucy.Core.Parsing.Nodes
{
    public record DocumentRootSyntaxNode(StatementListSyntaxNode StatementList, ComparableReadOnlyList<TriviaNode> TrailingTrivia) : SyntaxTreeNode
    {
        public static DocumentRootSyntaxNode ReadDocumentSyntaxNode(Code code)
        {
            var statementList = StatementListSyntaxNode.ReadStatementsWithoutBlock(code);
            var trailingTrivia = TriviaNode.ReadList(code);

            return new DocumentRootSyntaxNode(statementList, trailingTrivia);
        }
    }
}
