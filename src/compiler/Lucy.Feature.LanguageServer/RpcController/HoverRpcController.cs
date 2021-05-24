using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer.Services;
using Lucy.Core.Model.Syntax;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service]
    internal class HoverRpcController
    {
        private readonly CurrentWorkspace _currentWorkspace;

        public HoverRpcController(CurrentWorkspace currentWorkspace)
        {
            _currentWorkspace = currentWorkspace;
        }

        [JsonRpcFunction("textDocument/hover")]
        public Hover? TextDocumentHover(HoverParams input)
        {
            /*
            var document = _currentWorkspace.Get(input.TextDocument.Uri);
            if (document == null)
                return null!;

            int pos = document.TextDocument.ConvertPosition(new Position2D(input.Position.Line, input.Position.Character));
            var node = FindDepestNode(document.ParserResult.RootNode, pos);

            string? hint;
            if (node == null)
            {
                hint = "<null>";
            }
            else if (node is TokenNode tn)
            {
                hint = $"Token: \"{tn.Value}\"";
            }
            else
                hint = node.GetType().Name;
            
            return new Hover
            {
                Contents = new RpcMarkupContent
                {
                    Kind = RpcMarkupKind.Markdown,
                    Value = "```lucy\n" +
                    hint + "\n" +
                    "```"
                }
            };
            */
            return new Hover();
        }

        private SyntaxNode? FindDepestNode(SyntaxNode node, int pos)
        {
            /*
            foreach(var child in node.GetChildNodes().Select(x => x.Node))
            {
                if (pos >= child.Range.Position && pos < child.Range.Position + child.Range.Length)
                    return FindDepestNode(child, pos);
            }
            return node;
            */
            //TODO
            return null;
        }
    }

    public class HoverParams
    {
        /// <summary>
        /// The text document.
        /// </summary>
        public RpcTextDocumentIdentifier TextDocument { get; set; } = new RpcTextDocumentIdentifier();

        /// <summary>
        /// The position inside the text document.
        /// </summary>
        public RpcPosition Position { get; set; } = new RpcPosition();
    }

    public class Hover
    {
        /// <summary>
        /// The hover's content
        /// </summary>
        public RpcMarkupContent Contents { get; set; } = new RpcMarkupContent();

        /// <summary>
        /// An optional range is a range inside a text document that is used to visualize a hover, e.g.by changing the background color.
        /// </summary>
        public RpcRange? Range { get; set; }
    }
}
