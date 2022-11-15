using Lucy.Core.Model;

namespace Lucy.Core.Parsing;

public class TokenNodeParser
{
    public static TokenNodeBuilder Missing(string? errorMessage = null)
    {
        var token = new TokenNodeBuilder("");
        if (errorMessage != null)
            token.SyntaxErrors.Add(errorMessage);
        return token;
    }
}