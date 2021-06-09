using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using System;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Helper;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.Parser.Nodes.Token;

namespace Lucy.App.LanguageServer.Features.Hover
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
        public RpcHover? TextDocumentHover(RpcHoverParams input)
        {
            if (_currentWorkspace.Workspace == null)
                throw new Exception("No workspace loaded.");

            var document = _currentWorkspace.Workspace.Get(_currentWorkspace.ToWorkspacePath(input.TextDocument.Uri));
            if (document == null || document.SyntaxTree == null)
                return new RpcHover();

            SyntaxTreeNode? Walk(SyntaxTreeNode node)
            {
                foreach (var child in node.GetChildNodes())
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
                return new RpcHover();

            return new RpcHover
            {
                Contents = new RpcMarkupContent
                {
                    Kind = RpcMarkupKind.Plaintext,
                    Value = node.GetType().Name
                }
            };
        }
    }
}
