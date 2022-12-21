using Lucy.Core.Parsing.Nodes.Statements;
using Lucy.Core.Parsing.Nodes.Trivia;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes;

public static class DocumentRootSyntaxNodeParser
{
    public static DocumentRootSyntaxNode ReadDocumentSyntaxNode(Reader reader)
    {
        return reader.WithCache(nameof(DocumentRootSyntaxNodeParser), static r =>
        {
            var leadingTrivia = TriviaParser.Read(r);
            var statementList = StatementListSyntaxNodeParser.ReadStatementsWithoutBlock(r);

            return DocumentRootSyntaxNode.Create(leadingTrivia, statementList);
        });
    }
}