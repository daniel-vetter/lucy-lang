using System.Collections.Immutable;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Core.ProjectManagement;
using Lucy.App.LanguageServer.Features.Diagnoistics;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service(Lifetime.Singleton)]
    internal class TextDocumentController
    {
        private readonly CurrentWorkspace _currentWorkspace;
        private readonly DiagnosticsReporter _diagnosticsReporter;

        public TextDocumentController(CurrentWorkspace currentWorkspace, DiagnosticsReporter diagnosticsReporter)
        {
            _currentWorkspace = currentWorkspace;
            _diagnosticsReporter = diagnosticsReporter;
        }

        [JsonRpcFunction("textDocument/didOpen", deserializeParamterIntoSingleObject: false)]
        public async Task DidOpen(RpcTextDocumentItem textDocument)
        {
            _currentWorkspace.AddOrUpdate(textDocument.Uri, textDocument.Text);
            await _diagnosticsReporter.Report();
        }

        [JsonRpcFunction("textDocument/didClose", deserializeParamterIntoSingleObject: false)]
        public Task DidClose(RpcTextDocumentIdentifier textDocument)
        {
            return Task.CompletedTask;
        }

        [JsonRpcFunction("textDocument/didChange", deserializeParamterIntoSingleObject: false)]
        public async Task DidChange(RpcVersionedTextDocumentIdentifier textDocument, ImmutableArray<RpcTextDocumentContentChangeEvent> contentChanges)
        {
            foreach (var change in contentChanges)
            {
                if (change.Range == null)
                    _currentWorkspace.AddOrUpdate(textDocument.Uri, change.Text);
                else
                    _currentWorkspace.IncrementelUpdate(textDocument.Uri, new Range2D(
                        new Position2D(change.Range.Start.Line, change.Range.Start.Character),
                        new Position2D(change.Range.End.Line, change.Range.End.Character)), change.Text);
            }

            await _diagnosticsReporter.Report();
        }
    }
}
