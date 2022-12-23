using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Infrastructure.RpcServer;
using System.Collections.Immutable;
using System.Linq;

namespace Lucy.App.LanguageServer.Features.DocumentLink
{
    [Service(Lifetime.Singleton)]
    internal class DocumentLinkController
    {
        private readonly CurrentWorkspace _currentWorkspace;

        public DocumentLinkController(CurrentWorkspace currentWorkspace)
        {
            _currentWorkspace = currentWorkspace;
        }

        [JsonRpcFunction("textDocument/documentLink")]
        public ImmutableArray<RpcDocumentLink> TextDocumentDocumentLink(RpcDocumentLinkParams input)
        {
            var documentPath = _currentWorkspace.ToWorkspacePath(input.TextDocument.Uri);
            var imports = _currentWorkspace.Analysis.GetImports(documentPath);

            return imports.Valid
                .Select(x =>
                {
                    var range1D = _currentWorkspace.Analysis.GetRangeFromNodeId(x.ImportPathTokenNodeId);
                    var range2D = _currentWorkspace.ToRange2D(documentPath, range1D);

                    return new RpcDocumentLink
                    {
                        Range = range2D.ToRpcRange(),
                        Target = _currentWorkspace.ToSystemPath(x.Path),
                        Tooltip = x.Path
                    };
                })
                .ToImmutableArray();
        }
    }
}
