using Lucy.Core.Parser.Nodes.Token;
using System.Collections.Generic;

namespace Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration
{
    internal static class FunctionDeclarationModifierList
    {
        public static List<TokenNode> Read(Code code)
        {
            var l = new List<TokenNode>();

            while (true)
            {
                if (TokenNode.TryReadKeyword(code, "extern", out var externKeyword))
                {
                    l.Add(externKeyword);
                    continue;
                }

                if (TokenNode.TryReadKeyword(code, "cdecl", out var cdeclKeyword))
                {
                    l.Add(cdeclKeyword);
                    continue;
                }

                if (TokenNode.TryReadKeyword(code, "stdcall", out var stdcallKeyword))
                {
                    l.Add(stdcallKeyword);
                    continue;
                }

                break;
            }

            return l;
        }
    }
}
