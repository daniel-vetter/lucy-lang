using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    public class RpcDefinitionParams
    {
        /// <summary>
        /// The text document
        /// </summary>
        public required RpcTextDocumentIdentifier TextDocument { get; set; }

        /// <summary>
        /// The position inside the text document.
        /// </summary>
        public required RpcPosition Position { get; set; }
    }

    public class RpcLocationLink
    {
        /// <summary>
        /// Span of the origin of this link.
        ///
        /// Used as the underlined span for mouse interaction. Defaults to the word
        /// range at the mouse position.
        /// </summary>
        public RpcRange? OriginSelectionRange { get; set; }

        /// <summary>
        /// The target resource identifier of this link.
        /// </summary>
        public required SystemPath TargetUri { get; set; }

        /// <summary>
        /// The full target range of this link. If the target for example is a symbol
        /// then target range is the range enclosing this symbol not including
        /// leading/trailing whitespace but everything else like comments. This
        /// information is typically used to highlight the range in the editor.
        /// </summary>
        public required RpcRange TargetRange { get; set; }

        /// <summary>
        /// The range that should be selected and revealed when this link is being#
        /// followed, e.g the name of a function. Must be contained by the
        /// `targetRange`. See also `DocumentSymbol#range`
        /// </summary>
        public required RpcRange TargetSelectionRange { get; set; }
    }
}
