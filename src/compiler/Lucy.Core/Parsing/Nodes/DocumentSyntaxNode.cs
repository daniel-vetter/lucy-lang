using Lucy.Core.Parsing.Nodes.Statements;
using Lucy.Core.Parsing.Nodes.Trivia;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes;

public static class DocumentRootSyntaxNodeParser
{
    public static DocumentRootSyntaxNode ReadDocumentSyntaxNode(Reader reader)
    {
        var result =  reader.WithCache(nameof(DocumentRootSyntaxNodeParser), static r =>
        {
            var startToken = ReadDocumentStartToken(r);
            var statementList = StatementListSyntaxNodeParser.ReadStatementsWithoutBlock(r);

            return DocumentRootSyntaxNode.Create(startToken, statementList);
        });

        reader.Trim();

        return result;
    }

    private static TokenNode ReadDocumentStartToken(Reader reader)
    {
        return reader.WithCache(nameof(ReadDocumentStartToken), static r => TokenNode.Create("", TriviaParser.Read(r)));
    }
}