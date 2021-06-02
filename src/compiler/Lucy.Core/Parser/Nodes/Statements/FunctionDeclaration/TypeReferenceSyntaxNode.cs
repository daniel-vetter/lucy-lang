using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System.Diagnostics.CodeAnalysis;

namespace Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration
{
    public class TypeReferenceSyntaxNode : SyntaxTreeNode
    {
        public TypeReferenceSyntaxNode(SyntaxElement typeName)
        {
            TypeName = typeName;
        }

        public SyntaxElement TypeName { get; set; }

        public static bool TryRead(Code code, [NotNullWhen(true)] out TypeReferenceSyntaxNode? result)
        {
            if (!SyntaxElement.TryReadIdentifier(code, out var token))
            {
                result = null;
                return false;
            }
            
            result = new TypeReferenceSyntaxNode(token);
            return true;
        }
    }
}
