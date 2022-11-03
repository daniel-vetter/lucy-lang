using System.Collections.Generic;
using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using System;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public record FunctionDeclarationStatementSyntaxNode(
        ComparableReadOnlyList<SyntaxElement> Modifiers,
            StringConstantExpressionSyntaxNode? ExternLibraryName,
            StringConstantExpressionSyntaxNode? ExternFunctionName,
            SyntaxElement FunKeyword,
            SyntaxElement FunctionName,
            SyntaxElement OpenBraket,
            ComparableReadOnlyList<FunctionDeclarationParameterSyntaxNode> ParameterList,
            SyntaxElement CloseBraket,
            SyntaxElement ReturnTypeSeperator,
            TypeReferenceSyntaxNode ReturnType,
            StatementListSyntaxNode? Body) : StatementSyntaxNode, ICustomIdElementName
    {
        public static FunctionDeclarationStatementSyntaxNode? Read(Code code)
        {
            using var t = code.BeginTransaction();

            var modifier = FunctionDeclarationModifierList.Read(code);

            StringConstantExpressionSyntaxNode.TryRead(code, out var libraryNameToken);
            StringConstantExpressionSyntaxNode.TryRead(code, out var functionNameToken);

            if (!SyntaxElement.TryReadKeyword(code, "fun", out var funKeyword))
                return null;

            t.Commit();

            if (!SyntaxElement.TryReadIdentifier(code, out var functionName))
                functionName = SyntaxElement.Synthesize("Expected function name");

            if (!SyntaxElement.TryReadExact(code, "(", out var openBraket))
                openBraket = SyntaxElement.Synthesize("Expected '('");

            var parameterList = FunctionDeclarationParameterSyntaxNode.ReadList(code);

            if (!SyntaxElement.TryReadExact(code, ")", out var closeBraket))
                closeBraket = SyntaxElement.Synthesize("Expected ')'");

            if (!SyntaxElement.TryReadExact(code, ":", out var returnTypeSeperator))
                returnTypeSeperator = SyntaxElement.Synthesize("Expected ':'");

            if (!TypeReferenceSyntaxNode.TryRead(code, out var returnType))
                returnType = TypeReferenceSyntaxNode.Synthesize("Expected return type");

            StatementListSyntaxNode.TryReadStatementBlock(code, out var body);

            return new FunctionDeclarationStatementSyntaxNode(
                Modifiers: modifier,
                ExternLibraryName: libraryNameToken,
                ExternFunctionName: functionNameToken,
                FunKeyword: funKeyword,
                FunctionName: functionName,
                OpenBraket: openBraket,
                ParameterList: parameterList,
                CloseBraket: closeBraket,
                ReturnTypeSeperator: returnTypeSeperator,
                ReturnType: returnType,
                Body: body
            );
        }

        string ICustomIdElementName.CustomIdElementName => FunctionName.Token.Text;
    }
}
