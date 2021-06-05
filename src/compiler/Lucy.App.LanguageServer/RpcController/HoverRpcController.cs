using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using System;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Helper;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.Parser.Nodes.Token;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service(Lifetime.Singleton)]
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
            if (_currentWorkspace.Workspace == null)
                throw new Exception("No workspace loaded.");

            var document = _currentWorkspace.Workspace.Get(_currentWorkspace.ToWorkspacePath(input.TextDocument.Uri));
            if (document == null || document.SyntaxTree == null)
                return new Hover();

            SyntaxTreeNode? Walk(SyntaxTreeNode node)
            {
                foreach(var child in node.GetChildNodes())
                {
                    if (child.GetRange().Contains(input.Position.Line, input.Position.Character))
                    {
                        if (child is SyntaxElement)
                            return node;
                        else
                            return Walk(child);
                    }
                }

                return null;
            }

            var node = Walk(document.SyntaxTree);
            if (node == null)
                return new Hover();

            return new Hover
            {
                Contents = new RpcMarkupContent
                {
                    Kind = RpcMarkupKind.Plaintext,
                    Value = node.GetType().Name
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
