using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Stuff;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class FunctionDeclarationStatementSyntaxNodeParser
{
    public static FunctionDeclarationStatementSyntaxNode? Read(Reader reader)
    {
        return reader.WithCache(nameof(FunctionDeclarationStatementSyntaxNodeParser), static r =>
        {
            var modifier = FunctionDeclarationModifierList.Read(r);

            StringConstantExpressionSyntaxNodeParser.TryRead(r, out var libraryNameToken);
            StringConstantExpressionSyntaxNodeParser.TryRead(r, out var functionNameToken);

            if (!TokenNodeParser.TryReadKeyword(r, "fun", out var funKeyword))
                return null;

            if (!TokenNodeParser.TryReadIdentifier(r, out var functionName))
                functionName = TokenNodeParser.Missing("Expected function name");

            if (!TokenNodeParser.TryReadExact(r, "(", out var openBracket))
                openBracket = TokenNodeParser.Missing("Expected '('");

            var parameterList = FunctionDeclarationParameterSyntaxNodeParser.Read(r);

            if (!TokenNodeParser.TryReadExact(r, ")", out var closeBracket))
                closeBracket = TokenNodeParser.Missing("Expected ')'");

            if (!TypeAnnotationSyntaxNodeParser.TryRead(r, out var returnType))
                returnType = TypeAnnotationSyntaxNodeParser.Missing("Expected return type");

            var body = StatementListSyntaxNodeParser.TryReadStatementBlock(r);

            return FunctionDeclarationStatementSyntaxNode.Create(
                modifiers: modifier,
                externLibraryName: libraryNameToken,
                externFunctionName: functionNameToken,
                funKeyword: funKeyword,
                functionName: functionName,
                openBraket: openBracket,
                parameterList: parameterList,
                closeBraket: closeBracket,
                returnType: returnType,
                body: body
            );
        });
    }
}