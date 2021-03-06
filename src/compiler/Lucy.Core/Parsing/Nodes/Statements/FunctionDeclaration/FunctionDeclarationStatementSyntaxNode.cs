using System.Collections.Generic;
using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
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
            List<FunctionDeclarationParameterSyntaxNode> parameterList,
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
            ParameterList = parameterList;
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
        public List<FunctionDeclarationParameterSyntaxNode> ParameterList { get; }
        public SyntaxElement CloseBraket { get; set; }
        public SyntaxElement ReturnTypeSeperator { get; set; }
        public TypeReferenceSyntaxNode ReturnType { get; set; }
        public StatementListSyntaxNode? Body { get; }

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
                functionName = new SyntaxElement();

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
}
