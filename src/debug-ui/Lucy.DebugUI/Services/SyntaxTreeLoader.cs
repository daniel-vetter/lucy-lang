using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Lucy.DebugUI.Services
{
    public static class SyntaxTreeLoader
    {
        public static async Task<SyntaxNode> Load(string path)
        {
            var json = JsonConvert.DeserializeObject<JObject>(await File.ReadAllTextAsync(path));

            return (SyntaxNode)Convert(json);
        }

        private static Node Convert(JObject jsonNode)
        {
            var type = jsonNode["type"]?.Value<string>() ?? throw new Exception("Missing type property");
            var range = jsonNode["range"]?.Value<string>() ?? throw new Exception("Missing range property");

            var children = ImmutableArray.CreateBuilder<ChildNodeReference>();
            var jsonChildrenObject = jsonNode["childNodes"] as JObject;
            if (jsonChildrenObject != null)
            {
                foreach(JProperty jsonChild in jsonChildrenObject.Properties())
                {
                    children.Add(new ChildNodeReference(jsonChild.Name, Convert((JObject)jsonChild.Value)));
                }
            }
            
            if (type == "Statement")
            {
                var name = jsonNode["name"]?.Value<string>() ?? throw new Exception("Missing name property");
                return new StatementSyntaxNode(name, children.ToImmutable(), range);
            }

            if (type == "Expression")
            {
                var name = jsonNode["name"]?.Value<string>() ?? throw new Exception("Missing name property");
                return new ExpressionSyntaxNode(name, children.ToImmutable(), range);
            }

            if (type == "Other")
            {
                var name = jsonNode["name"]?.Value<string>() ?? throw new Exception("Missing name property");
                return new SyntaxNode(name, children.ToImmutable(), range);
            }

            if (type == "Token")
            {
                var value = jsonNode["value"]?.Value<string>() ?? throw new Exception("Missing token value");
                return new TokenNode(value, range);
            }


            throw new Exception("Unsupported type: " + type);
        }
    }

    public abstract record Node(string Range);
    public record SyntaxNode(string Name, ImmutableArray<ChildNodeReference> ChildNodes, string Range) : Node(Range);
    public record StatementSyntaxNode(string Name, ImmutableArray<ChildNodeReference> ChildNodes, string Range) : SyntaxNode(Name, ChildNodes, Range);
    public record ExpressionSyntaxNode(string Name, ImmutableArray<ChildNodeReference> ChildNodes, string Range) : SyntaxNode(Name, ChildNodes, Range);
    public record TokenNode(string Value, string Range) : Node(Range);
    public record ChildNodeReference(string Name, Node Node);

    
}
