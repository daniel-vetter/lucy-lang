using System.Collections.Generic;
using Lucy.Core.Parser.Nodes.Token;
using Lucy.Core.Parser.Nodes.Expressions.Unary;

namespace Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration
{
    public class FunctionDeclarationStatementSyntaxNode : StatementSyntaxNode
    {
        public FunctionDeclarationStatementSyntaxNode(
            List<SyntaxElement> modifiers,
            StringConstantExpressionSyntaxNode? externLibraryName,
            StringConstantExpressionSyntaxNode? externFunctionName,
            SyntaxElement funKeyword,
            SyntaxElement functionName,
            SyntaxElement openBraket,
            List<FunctionDeclarationParameterSyntaxNode> functionDeclarationParameterNodes,
            SyntaxElement closeBraket,
            SyntaxElement returnTypeSeperator,
            TypeReferenceSyntaxNode returnType,
            StatementListSyntaxNode? body)
        {
            Modifiers = modifiers;
            ExternLibraryName = externLibraryName;
            ExternFunctionName = externFunctionName;
            FunKeyword = funKeyword;
            FunctionName = functionName;
            OpenBraket = openBraket;
            FunctionDeclarationParameterNodes = functionDeclarationParameterNodes;
            CloseBraket = closeBraket;
            ReturnTypeSeperator = returnTypeSeperator;
            ReturnType = returnType;
            Body = body;
        }

        public List<SyntaxElement> Modifiers { get; }
        public StringConstantExpressionSyntaxNode? ExternLibraryName { get; set; }
        public StringConstantExpressionSyntaxNode? ExternFunctionName { get; set; }
        public SyntaxElement FunKeyword { get; set; }
        public SyntaxElement FunctionName { get; set; }
        public SyntaxElement OpenBraket { get; set; }
        public List<FunctionDeclarationParameterSyntaxNode> FunctionDeclarationParameterNodes { get; }
        public SyntaxElement CloseBraket { get; set; }
        public SyntaxElement ReturnTypeSeperator { get; set; }
        public TypeReferenceSyntaxNode ReturnType { get; set; }
        public StatementListSyntaxNode? Body { get; }

        public static FunctionDeclarationStatementSyntaxNode? Read(Code code)
        {
            var start = code.Position;

            var modifier = FunctionDeclarationModifierList.Read(code);

            StringConstantExpressionSyntaxNode.TryRead(code, out var libraryNameToken);
            StringConstantExpressionSyntaxNode.TryRead(code, out var functionNameToken);

            if (!SyntaxElement.TryReadKeyword(code, "fun", out var funKeyword))
            {
                code.SeekTo(start);
                return null;
            }

            if (!SyntaxElement.TryReadIdentifier(code, out var functionName))
            {
                code.ReportError("Expected method name", code.Position);
                return null;
            }

            if (!SyntaxElement.TryReadExact(code, "(", out var openBraket))
            {
                code.ReportError("Expected '('", code.Position);
                return null;
            }

            var parameterList = FunctionDeclarationParameterSyntaxNode.ReadList(code);

            if (!SyntaxElement.TryReadExact(code, ")", out var closeBraket))
            {
                code.ReportError("Expected ')'", code.Position);
                return null;
            }

            if (!SyntaxElement.TryReadExact(code, ":", out var returnTypeSeperator))
            {
                code.ReportError("Expected ':'", code.Position);
                return null;
            }

            if (!TypeReferenceSyntaxNode.TryRead(code, out var returnType))
            {
                code.ReportError("Expected return type", code.Position);
                return null;
            }

            StatementListSyntaxNode.TryReadStatementBlock(code, out var body);

            return new FunctionDeclarationStatementSyntaxNode(
                modifiers: modifier,
                externLibraryName: libraryNameToken,
                externFunctionName: functionNameToken,
                funKeyword: funKeyword,
                functionName: functionName,
                openBraket: openBraket,
                functionDeclarationParameterNodes: parameterList,
                closeBraket: closeBraket,
                returnTypeSeperator: returnTypeSeperator,
                returnType: returnType,
                body: body
            );
        }
    }
}
