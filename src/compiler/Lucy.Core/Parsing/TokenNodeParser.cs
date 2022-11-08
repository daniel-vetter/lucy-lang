using Lucy.Core.Model;

namespace Lucy.Core.Parsing;

public class TokenNodeParser
{
    public static TokenNode Missing(string? errorMessage = null)
    {
        var token = new TokenNode("");
        if (errorMessage != null)
            token.SyntaxErrors.Add(errorMessage);
        return token;
    }
}