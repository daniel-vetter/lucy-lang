using System.Collections.Immutable;
using System.Xml;

namespace Lucy.Core.SourceGenerator
{
    public class ConfigLoader
    {
        public static Config GetConfig(string xml)
        {
            var config = new XmlDocument();
            config.LoadXml(xml);

            var c = new Config();
            c.Namespace = config.DocumentElement.Attributes["namespace"].Value;
            foreach (XmlElement child in config.DocumentElement.ChildNodes)
            {
                var node = c.AddNode(child.Name);
                node.BasedOn = child.HasAttribute("basedOn") ? child.Attributes["basedOn"].Value : null;

                foreach (XmlElement prop in child.ChildNodes)
                {
                    var p = node.AddProperty(prop.Name, prop.Attributes["type"].Value);
                    p.Init = prop.HasAttribute("init") ? prop.Attributes["init"].Value : null;
                    p.IsOptional = prop.HasAttribute("optional") ? prop.Attributes["optional"].Value == "true" : false;
                    p.IsList = prop.HasAttribute("list") ? prop.Attributes["list"].Value == "true" : false;
                }
            }
            return c;
        }
    }

    public class Config
    {
        private List<Node> _nodes = new();
        private Dictionary<string, Node> _nodesByName = new();

        public Node AddNode(string name)
        {
            var node = new Node(this, name);
            _nodes.Add(node);
            _nodesByName.Add(name, node);
            return node;
        }

        public Node GetNodeByName(string name) => _nodesByName[name];
        public bool ContainsNode(string name) => _nodesByName.ContainsKey(name);
        public IReadOnlyList<Node> Nodes => _nodes;

        public string Namespace { get; set; } = "";
    }

    public class Node
    {
        public Node(Config config, string name)
        {
            Config = config;
            Name = name;
        }

        private List<NodeProperty> _props = new();

        public Config Config { get; }
        public string Name { get; }
        public string? BasedOn { get; set; }
        public bool IsRoot => BasedOn == null;
        public bool IsTopMost => !Config.Nodes.Any(x => x.BasedOn == Name);

        public IReadOnlyList<NodeProperty> Properties => _props;

        public IEnumerable<NodeProperty> BaseProperties
        {
            get
            {
                var currentNode = this;
                if (currentNode.BasedOn == null)
                    yield break;

                while (true)
                {
                    currentNode = Config.GetNodeByName(currentNode.BasedOn);

                    foreach (var p in currentNode.Properties)
                        yield return p;

                    if (currentNode.BasedOn == null)
                        break;
                }
            }
        }

        public IEnumerable<NodeProperty> AllProperties
        {
            get
            {
                var currentNode = this;
                while (true)
                {
                    foreach (var p in currentNode.Properties)
                        yield return p;

                    if (currentNode.BasedOn == null)
                        break;

                    currentNode = Config.GetNodeByName(currentNode.BasedOn);
                }
            }
        }

        public NodeProperty AddProperty(string name, string type)
        {
            var prop = new NodeProperty(this, name, type);
            _props.Add(prop);
            return prop;
        }
    }

    public class NodeProperty
    {
        public NodeProperty(Node node, string name, string type)
        {
            Node = node;
            Name = name;
            Type = type;
        }

        public Node Node { get; }
        public string Name { get; }
        public string Type{ get; }
        public bool TypeIsNode => Node.Config.ContainsNode(Type);
        public bool IsList { get; set; }
        public bool IsOptional { get; set; }
        public string? Init { get; set; }
    }
}
