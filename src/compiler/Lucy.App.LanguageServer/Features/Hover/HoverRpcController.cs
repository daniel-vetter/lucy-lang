using Lucy.Infrastructure.RpcServer;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Handler;

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
        var node = _currentWorkspace.Analysis.GetNodeAtPosition(documentPath, position);

        if (node == null)
            return new RpcHover();

        string? tooltip = null;

        while (true)
        {
            if (node == null)
                break;

            if (node is TypeReferenceSyntaxNode typeReferenceSyntaxNode)
            {
                var typeInfo = _currentWorkspace.Analysis.GetTypeInfoFromTypeReferenceId(typeReferenceSyntaxNode.NodeId);
                tooltip = typeInfo == null 
                    ? "Unknown type" 
                    : typeInfo.Name;
                break;
            }

            if (node is ExpressionSyntaxNode expressionSyntaxNode)
            {
                tooltip = _currentWorkspace.Analysis.GetExpressionType(expressionSyntaxNode.NodeId)?.Name ?? "Unknown type";
                break;
            }
            
            node = _currentWorkspace.Analysis.GetParentNode(node.NodeId);
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