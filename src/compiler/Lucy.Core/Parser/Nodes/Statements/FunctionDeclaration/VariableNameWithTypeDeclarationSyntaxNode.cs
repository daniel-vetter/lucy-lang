using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration
{
    public class VariableNameWithTypeDeclarationSyntaxNode : SyntaxNode
    {
        public VariableNameWithTypeDeclarationSyntaxNode(TokenNode variableName, TokenNode seperator, TypeReferenceSyntaxNode typeReference)
        {
            VariableName = variableName;
            Seperator = seperator;
            TypeReference = typeReference;
        }

        public TokenNode VariableName { get; set; }
        public TokenNode Seperator { get; set; }
        public TypeReferenceSyntaxNode TypeReference { get; set; }

        public static bool Read(Code code, [NotNullWhen(true)] out VariableNameWithTypeDeclarationSyntaxNode? result)
        {
            var start = code.Position;
            result = null;

            if (!TokenNode.TryReadIdentifier(code, out var variableName))
                return false;

            if (!TokenNode.TryReadExact(code, ":", out var seperator))
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
