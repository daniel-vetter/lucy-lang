using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.SemanticAnalysis.Infrastructure;
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
        var nodeId = _currentWorkspace.Analysis.Get<Ranges>().GetNodeAtPosition(documentPath, position);

        if (nodeId == null)
            return new RpcHover();
        
        TypeInfo? typeInfo = null;
        while (nodeId != null)
        {
            if (nodeId is INodeId<TypeReferenceSyntaxNode>
                or INodeId<VariableDeclarationStatementSyntaxNode>
                or INodeId<ExpressionSyntaxNode>)
            {
                typeInfo = _currentWorkspace.Analysis.Get<Types>().GetTypeInfo(nodeId);
                break;
            }

            nodeId = _currentWorkspace.Analysis.Get<Nodes>().GetParentNodeId(nodeId);
        }

        var tooltip = typeInfo == null
            ? "Type: Unknown type"
            : "Type: " + typeInfo.Name + "";

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