using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using System;
using Lucy.Core.Helper;
using Lucy.Core.Parsing.Nodes.Token;
using Lucy.Core.Parsing;
using System.Text;

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

            var stack = TreeAnalyzer.GetStack(document.SyntaxTree, input.Position.Line, input.Position.Character);
            var sb = new StringBuilder();
            foreach(var node in stack)
            {
                sb.AppendLine(" * " + node.GetType().Name);
                foreach(var annotation in node.Annotations)
                {
                    sb.AppendLine("   * " + annotation.Key.Name);
                }
                sb.AppendLine();
            }

            return new RpcHover
            {
                Contents = new RpcMarkupContent
                {
                    Kind = RpcMarkupKind.Markdown,
                    Value = sb.ToString()
                }
            };
        }
    }
}
