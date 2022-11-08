using System.Xml;
using System.Collections.Immutable;

namespace Lucy.Core.SourceGenerator
{
    public class ConfigLoader
    {
        public static Config GetConfig(string xml)
        {
            var config = new XmlDocument();
            config.LoadXml(xml);

            var ns = config.DocumentElement.Attributes["namespace"].Value;

            var r = ImmutableArray.CreateBuilder<Node>();
            foreach (XmlElement child in config.DocumentElement.ChildNodes)
            {
                var name = child.Name;
                var basedOn = child.HasAttribute("basedOn") ? child.Attributes["basedOn"].Value : throw new Exception("Missing 'basedOn' attribute on '" + name + "'");
                var isAbstract = child.HasAttribute("abstract") ? child.Attributes["abstract"].Value == "true" : false;

                var b = ImmutableArray.CreateBuilder<NodeProperty>();
                foreach (XmlElement prop in child.ChildNodes)
                {
                    var propName = prop.Name;
                    var propType = prop.Attributes["type"].Value;
                    var isList = prop.HasAttribute("list") ? prop.Attributes["list"].Value == "true" : false;
                    b.Add(new NodeProperty(propName, propType, isList));
                }

                r.Add(new Node(name, basedOn, isAbstract, b.ToImmutable()));
            }
            return new Config(ns, r.ToImmutable());
        }
    }

    public record Config(string Namespace, ImmutableArray<Node> Nodes);

    public record Node(
        string Name,
        string BasedOn,
        bool Abstract,
        ImmutableArray<NodeProperty> Properties
    );

    public record NodeProperty(string Name, string Type, bool IsList);


}
