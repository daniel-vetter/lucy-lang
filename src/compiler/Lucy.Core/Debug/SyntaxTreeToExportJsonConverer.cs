using System;
using Lucy.Core.Helper;
using Newtonsoft.Json.Linq;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Statements;
using Lucy.Core.Parser.Nodes.Expressions;
using Lucy.Core.Parser.Nodes.Token;

namespace Lucy.Feature.LanguageServer.Services
{
    public class SyntaxTreeToExportJsonConverer
    {
        public static JObject Convert(SyntaxNode node)
        {
            if (node is SyntaxNode SyntaxNode)
            {
                var type = SyntaxNode switch
                {
                    StatementSyntaxNode => "Statement",
                    ExpressionSyntaxNode => "Expression",
                    _ => "Other"
                };

                var childNodes = new JObject();
                foreach(var childNode in node.GetChildNodes())
                    childNodes.Add(childNode.Name, Convert(childNode.Node));

                return JObject.FromObject(new
                {
                    type = type,
                    name = SyntaxNode.GetType().Name,
                    //range = SyntaxNode.Range.Position + "-" + (SyntaxNode.Range.Position + SyntaxNode.Range.Length), //TODO
                    childNodes = childNodes
                });
            }

            if (node is TokenNode tokenNode)
            {
                return JObject.FromObject(new 
                {
                    type = "Token",
                    value = tokenNode.Value,
                    //range = tokenNode.Range.Position + "-" + (tokenNode.Range.Position + tokenNode.Range.Length) //TODO
                });
            }

            throw new NotSupportedException("Unsupported node: " + node);
        }
    }
}
