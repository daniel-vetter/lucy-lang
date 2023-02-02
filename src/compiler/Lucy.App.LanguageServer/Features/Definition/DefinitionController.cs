using System;
using System.Collections.Immutable;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
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

            var node = _currentWorkspace.Analysis.Get<RangeResolver>().GetNodeIdAtPosition(workspacePath, position);
            if (node == null)
                return ImmutableArray<RpcLocationLink>.Empty;
            
            while (true)
            {
                if (node is INodeId<FunctionCallExpressionSyntaxNode> functionCall)
                {
                    var bestMatch = _currentWorkspace.Analysis.Get<Functions>().GetBestFunctionCallTarget(functionCall);
                    var targets = bestMatch == null
                        ? _currentWorkspace.Analysis.Get<Functions>().GetAllPossibleFunctionCallTargets(functionCall)
                        : new ComparableReadOnlyList<INodeId<SyntaxTreeNode>>(new[] { bestMatch });
                    
                    var result = ImmutableArray.CreateBuilder<RpcLocationLink>();

                    foreach (var target in targets)
                    {
                        if (target is INodeId<FunctionDeclarationStatementSyntaxNode> fd)
                        {
                            var flat = _currentWorkspace.Analysis.Get<Flats>().GetFlatFunctionDeclaration(fd);
                            var link = new RpcLocationLink
                            {
                                TargetUri = _currentWorkspace.ToSystemPath(flat.NodeId.DocumentPath),
                                TargetRange = _currentWorkspace.ToRange2D(flat.NodeId.DocumentPath,
                                    _currentWorkspace.Analysis.Get<RangeResolver>().GetTrimmedRangeFromNodeId(flat.NodeId)).ToRpcRange(),
                                TargetSelectionRange = _currentWorkspace.ToRange2D(flat.NodeId.DocumentPath,
                                    _currentWorkspace.Analysis.Get<RangeResolver>().GetTrimmedRangeFromNodeId(flat.Name.NodeId)).ToRpcRange()
                            };
                            result.Add(link);
                        }
                    }

                    return result.ToImmutable();
                }

                node = _currentWorkspace.Analysis.Get<Nodes>().GetParentNodeId(node);
                if (node == null)
                    break;
            }

            return ImmutableArray<RpcLocationLink>.Empty;
        }
    }
}
