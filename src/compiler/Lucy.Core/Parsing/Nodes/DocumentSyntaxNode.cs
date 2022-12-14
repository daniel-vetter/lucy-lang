using Lucy.Core.Parsing.Nodes.Statements;
using Lucy.Core.Parsing.Nodes.Trivia;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes;

public static class DocumentRootSyntaxNodeParser
{
    public static DocumentRootSyntaxNodeBuilder ReadDocumentSyntaxNode(Reader reader)
    {
        return reader.WithCache(nameof(DocumentRootSyntaxNodeParser), static code =>
        {
            var leadingTrivia = TriviaParser.Read(code);
            var statementList = StatementListSyntaxNodeParser.ReadStatementsWithoutBlock(code);
            return new DocumentRootSyntaxNodeBuilder(leadingTrivia, statementList);
        });
    }
}