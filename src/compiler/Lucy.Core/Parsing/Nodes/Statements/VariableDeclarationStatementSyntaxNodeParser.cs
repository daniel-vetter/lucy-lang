﻿using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes.Expressions;
using Lucy.Core.Parsing.Nodes.Stuff;
using Lucy.Core.Parsing.Nodes.Token;

namespace Lucy.Core.Parsing.Nodes.Statements
{
    public static class VariableDeclarationStatementSyntaxNodeParser
    {
        public static VariableDeclarationStatementSyntaxNode? Read(Reader reader)
        {
            return reader.WithCache(nameof(VariableDeclarationStatementSyntaxNodeParser), static r =>
            {
                if (!TokenNodeParser.TryReadKeyword(r, "var", out var varKeyword))
                    return null;

                if (!VariableDefinitionSyntaxNodeParser.TryReadVariableDefinitionSyntaxNode(r, out var variableDefinition))
                    variableDefinition = VariableDefinitionSyntaxNodeParser.Missing("Variable definition expected");

                if (!TokenNodeParser.TryReadExact(r, "=", out var equalSign))
                    equalSign = TokenNodeParser.Missing("'=' expected");

                if (!ExpressionSyntaxNodeParser.TryRead(r, out var expression))
                    expression = ExpressionSyntaxNodeParser.Missing("Expression expected");

                return VariableDeclarationStatementSyntaxNode.Create(varKeyword, variableDefinition, equalSign, expression);
            });
        }
    }
}
