using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Model;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;

public static class FunctionDeclarationStatementSyntaxNodeParser
{
    public static FunctionDeclarationStatementSyntaxNodeBuilder? Read(Code code)
    {
        using var t = code.BeginTransaction();

        var modifier = FunctionDeclarationModifierList.Read(code);

        StringConstantExpressionSyntaxNodeParser.TryRead(code, out var libraryNameToken);
        StringConstantExpressionSyntaxNodeParser.TryRead(code, out var functionNameToken);

        if (!SyntaxElementParser.TryReadKeyword(code, "fun", out var funKeyword))
            return null;

        t.Commit();

        if (!SyntaxElementParser.TryReadIdentifier(code, out var functionName))
            functionName = SyntaxElementParser.Missing("Expected function name");

        if (!SyntaxElementParser.TryReadExact(code, "(", out var openBraket))
            openBraket = SyntaxElementParser.Missing("Expected '('");

        var parameterList = FunctionDeclarationParameterSyntaxNodeParser.ReadList(code);

        if (!SyntaxElementParser.TryReadExact(code, ")", out var closeBraket))
            closeBraket = SyntaxElementParser.Missing("Expected ')'");

        if (!SyntaxElementParser.TryReadExact(code, ":", out var returnTypeSeperator))
            returnTypeSeperator = SyntaxElementParser.Missing("Expected ':'");

        if (!TypeReferenceSyntaxNodeParser.TryRead(code, out var returnType))
            returnType = TypeReferenceSyntaxNodeParser.Synthesize("Expected return type");

        StatementListSyntaxNodeParser.TryReadStatementBlock(code, out var body);

        return new FunctionDeclarationStatementSyntaxNodeBuilder(
            modifiers: modifier,
            externLibraryName: libraryNameToken,
            externFunctionName: functionNameToken,
            funKeyword: funKeyword,
            functionName: functionName,
            openBraket: openBraket,
            parameterList: parameterList,
            closeBraket: closeBraket,
            returnTypeSeperator: returnTypeSeperator,
            returnType: returnType,
            body: body
        );
    }
}