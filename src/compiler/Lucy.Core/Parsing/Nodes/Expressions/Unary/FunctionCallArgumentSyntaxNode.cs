using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parsing.Nodes.Expressions.Unary
{
    public class FunctionCallArgumentSyntaxNodeParser
    {
        public static List<FunctionCallArgumentSyntaxNode> Read(Code code)
        {
            var result = new List<FunctionCallArgumentSyntaxNode>();
            while (true)
            {
                if (!ExpressionSyntaxNodeParser.TryRead(code, out var expression))
                    break;

                SyntaxElementParser.TryReadExact(code, ",", out var seperator);

                result.Add(new FunctionCallArgumentSyntaxNode(expression, seperator));

                if (seperator == null)
                    break;
            }

            return result;
        }
    }
}
