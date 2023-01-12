using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Infrastructure.RpcServer;

namespace Lucy.App.LanguageServer.Features.Hover;

[Service(Lifetime.Singleton)]
internal class HoverRpcController
{
    private readonly CurrentWorkspace _currentWorkspace;

    public HoverRpcController(CurrentWorkspace currentWorkspace)
    {
        _currentWorkspace = currentWorkspace;
    }

    [JsonRpcFunction("textDocument/hover")]
    public RpcHover TextDocumentHover(RpcHoverParams input)
    {
        var documentPath = _currentWorkspace.ToWorkspacePath(input.TextDocument.Uri);
        var position = _currentWorkspace.ToPosition1D(documentPath, input.Position.ToPosition2D());
        var nodeId = _currentWorkspace.Analysis.GetNodeAtPosition(documentPath, position);

        if (nodeId == null)
            return new RpcHover();

        var tooltip = "NodeId: " + nodeId + "\\\n";
        tooltip += "Position: " + input.Position + ", " + position.Position + "\\\n";

        while (true)
        {
            if (nodeId == null)
                break;

            if (nodeId is INodeId<TypeReferenceSyntaxNode> typeReferenceSyntaxNode)
            {
                var typeInfo = _currentWorkspace.Analysis.GetTypeInfoFromTypeReferenceId(typeReferenceSyntaxNode);
                tooltip += typeInfo == null
                    ? "Type: Unknown type\\\n"
                    : "Type: " + typeInfo.Name + "\\\n";
                break;
            }

            if (nodeId is INodeId<ExpressionSyntaxNode> expressionSyntaxNode)
            {
                tooltip += "Type: " + (_currentWorkspace.Analysis.GetExpressionType(expressionSyntaxNode)?.Name ?? "Unknown type") + "\\\n ";
                break;
            }
            
            nodeId = _currentWorkspace.Analysis.GetParentNodeId(nodeId);
        }

        return new RpcHover
        {
            Contents = new RpcMarkupContent
            {
                Kind = RpcMarkupKind.Markdown,
                Value = $"""{tooltip}"""
            }
        };
    }
}