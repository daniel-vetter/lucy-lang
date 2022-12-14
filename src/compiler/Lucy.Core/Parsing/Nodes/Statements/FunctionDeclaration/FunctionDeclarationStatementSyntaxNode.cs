using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class FunctionDeclarationStatementSyntaxNodeParser
{
    public static FunctionDeclarationStatementSyntaxNodeBuilder? Read(Reader reader)
    {
        return reader.WithCache(nameof(FunctionDeclarationStatementSyntaxNodeParser), static code =>
        {
            var modifier = FunctionDeclarationModifierList.Read(code);

            StringConstantExpressionSyntaxNodeParser.TryRead(code, out var libraryNameToken);
            StringConstantExpressionSyntaxNodeParser.TryRead(code, out var functionNameToken);

            if (!TokenNodeParser.TryReadKeyword(code, "fun", out var funKeyword))
                return null;

            if (!TokenNodeParser.TryReadIdentifier(code, out var functionName))
                functionName = TokenNodeParser.Missing("Expected function name");

            if (!TokenNodeParser.TryReadExact(code, "(", out var openBracket))
                openBracket = TokenNodeParser.Missing("Expected '('");

            var parameterList = FunctionDeclarationParameterSyntaxNodeParser.Read(code);

            if (!TokenNodeParser.TryReadExact(code, ")", out var closeBracket))
                closeBracket = TokenNodeParser.Missing("Expected ')'");

            if (!TypeAnnotationSyntaxNodeParser.TryRead(code, out var returnType))
                returnType = TypeAnnotationSyntaxNodeParser.Missing("Expected return type");

            var body = StatementListSyntaxNodeParser.TryReadStatementBlock(code);

            return new FunctionDeclarationStatementSyntaxNodeBuilder(
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