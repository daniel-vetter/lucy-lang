using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service(Lifetime.Singleton)]
    internal class HoverRpcController
    {
        [JsonRpcFunction("textDocument/hover")]
        public Hover? TextDocumentHover(HoverParams input)
        {
            return new Hover
            {
                Contents = new RpcMarkupContent
                {
                    Kind = RpcMarkupKind.Plaintext,
                    Value = "Hello World"
                }
            };
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
