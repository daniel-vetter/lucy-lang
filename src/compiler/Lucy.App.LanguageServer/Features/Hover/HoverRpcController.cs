using Lucy.Infrastructure.RpcServer;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
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

        return new RpcHover
        {
            Contents = new RpcMarkupContent
            {
                Kind = RpcMarkupKind.Markdown,
                Value = $"""<span style="color:blue">NodeId: {node.NodeId}</span>"""
            }
        };
    }
}