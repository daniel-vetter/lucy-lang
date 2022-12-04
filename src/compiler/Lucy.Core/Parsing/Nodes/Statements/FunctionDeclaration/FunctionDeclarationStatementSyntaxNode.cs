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

        if (!SyntaxElementParser.TryReadExact(code, "(", out var openBracket))
            openBracket = SyntaxElementParser.Missing("Expected '('");

        var parameterList = FunctionDeclarationParameterSyntaxNodeParser.ReadList(code);

        if (!SyntaxElementParser.TryReadExact(code, ")", out var closeBracket))
            closeBracket = SyntaxElementParser.Missing("Expected ')'");
        
        if (!TypeAnnotationSyntaxNodeParser.TryRead(code, out var returnType))
            returnType = TypeAnnotationSyntaxNodeParser.Missing("Expected return type");

        StatementListSyntaxNodeParser.TryReadStatementBlock(code, out var body);

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
    }
}