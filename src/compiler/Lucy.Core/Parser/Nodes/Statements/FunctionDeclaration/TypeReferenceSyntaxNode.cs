using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration
{
    public class TypeReferenceSyntaxNode : SyntaxNode
    {
        public TypeReferenceSyntaxNode(TokenNode typeName)
        {
            TypeName = typeName;
        }

        public TokenNode TypeName { get; set; }

        public static bool TryRead(Code code, [NotNullWhen(true)] out TypeReferenceSyntaxNode? result)
        {
            if (!TokenNode.TryReadIdentifier(code, out var token))
            {
                result = null;
                return false;
            }
            
            result = new TypeReferenceSyntaxNode(token);
            return true;
        }
    }
}
