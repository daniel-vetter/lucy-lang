using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration
{
    public class VariableNameWithTypeDeclarationSyntaxNode : SyntaxTreeNode
    {
        public VariableNameWithTypeDeclarationSyntaxNode(SyntaxElement variableName, SyntaxElement seperator, TypeReferenceSyntaxNode typeReference)
        {
            VariableName = variableName;
            Seperator = seperator;
            TypeReference = typeReference;
        }

        public SyntaxElement VariableName { get; set; }
        public SyntaxElement Seperator { get; set; }
        public TypeReferenceSyntaxNode TypeReference { get; set; }

        public static bool Read(Code code, [NotNullWhen(true)] out VariableNameWithTypeDeclarationSyntaxNode? result)
        {
            var start = code.Position;
            result = null;

            if (!SyntaxElement.TryReadIdentifier(code, out var variableName))
                return false;

            if (!SyntaxElement.TryReadExact(code, ":", out var seperator))
            {
                code.SeekTo(start);
                return false;
            }

            if (!TypeReferenceSyntaxNode.TryRead(code, out var typeReference))
            {
                code.SeekTo(start);
                return false;
            }

            result = new VariableNameWithTypeDeclarationSyntaxNode(variableName, seperator, typeReference);
            return true;
        }
    }
}
