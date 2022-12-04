using Lucy.App.LanguageServer.Models;
using Lucy.Infrastructure.RpcServer;
using System.Collections.Immutable;
using System.Linq;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.SemanticAnalysis.Handler;
using System.Collections.Generic;

namespace Lucy.App.LanguageServer.Features.Completion
{
    [Service(Lifetime.Singleton)]
    internal class CompletionController
    {
        private readonly CurrentWorkspace _currentWorkspace;

        public CompletionController(CurrentWorkspace currentWorkspace)
        {
            _currentWorkspace = currentWorkspace;
        }

        [JsonRpcFunction("textDocument/completion")]
        public RpcCompletionList TextDocumentCompletion(RpcCompletionParams input)
        {
            var workspacePath = _currentWorkspace.ToWorkspacePath(input.TextDocument.Uri);
            var position = _currentWorkspace.ToPosition1D(workspacePath, input.Position.ToPosition2D());

            var node = _currentWorkspace.Analysis.GetNodeAtPosition(workspacePath, position);

            if (node == null)
                return new RpcCompletionList();

            var functions = _currentWorkspace.Analysis.GetAvailableFunctionsInScope(node.NodeId);
            var variables = _currentWorkspace.Analysis.GetAvailableVariablesInScope(node.NodeId);

            var result = new List<RpcCompletionItem>();
            foreach (var function in functions)
            {
                result.Add(new RpcCompletionItem
                {
                    Kind = RpcCompletionItemKind.Function,
                    Label = function.Name.Text
                });
            }

            foreach (var function in variables)
            {
                result.Add(new RpcCompletionItem
                {
                    Kind = RpcCompletionItemKind.Variable,
                    Label = function.Name.Text
                });
            }

            return new RpcCompletionList
            {
                Items = result.ToImmutableArray()
            };
        }
    }
}
