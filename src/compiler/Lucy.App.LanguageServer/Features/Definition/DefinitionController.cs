using System.Collections.Immutable;
using System.Linq;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Infrastructure.RpcServer;

namespace Lucy.App.LanguageServer.Features.Definition
{
    [Service(Lifetime.Singleton)]
    internal class DefinitionController
    {
        private readonly CurrentWorkspace _currentWorkspace;

        public DefinitionController(CurrentWorkspace currentWorkspace)
        {
            _currentWorkspace = currentWorkspace;
        }

        [JsonRpcFunction("textDocument/definition")]
        public ImmutableArray<RpcLocationLink> TextDocumentDefinition(RpcDefinitionParams input)
        {
            var workspacePath = _currentWorkspace.ToWorkspacePath(input.TextDocument.Uri);
            var position = _currentWorkspace.ToPosition1D(workspacePath, input.Position.ToPosition2D());

            var node = _currentWorkspace.Analysis.GetNodeAtPosition(workspacePath, position);
            if (node == null)
                return ImmutableArray<RpcLocationLink>.Empty;

            
            while (true)
            {
                if (node is FunctionCallExpressionSyntaxNode functionCall)
                {
                    var bestMatch = _currentWorkspace.Analysis.GetBestMatchingFunctionsFromFunctionCall(functionCall.NodeId);
                    if (bestMatch != null)
                    {

                    }

                    var allMatches = _currentWorkspace.Analysis.GetAllMatchingFunctionsFromFunctionCall(functionCall.NodeId);
                    return allMatches.Select(x => new RpcLocationLink
                        {
                            TargetUri = _currentWorkspace.ToSystemPath(x.NodeId.DocumentPath),
                            TargetRange = _currentWorkspace.ToRange2D(x.NodeId.DocumentPath, _currentWorkspace.Analysis.GetRangeFromNodeId(x.NodeId)).ToRpcRange(),
                            TargetSelectionRange = _currentWorkspace.ToRange2D(x.NodeId.DocumentPath, _currentWorkspace.Analysis.GetRangeFromNodeId(x.NodeId)).ToRpcRange()
                    })
                        .ToImmutableArray();
                }

                node = _currentWorkspace.Analysis.GetParentNode(node.NodeId);
                if (node == null) 
                    break;
            }

            return ImmutableArray<RpcLocationLink>.Empty;
        }
    }
}
