using System.Collections.Immutable;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Feature.LanguageServer.Services;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service]
    internal class TextDocumentController
    {
        private readonly CurrentWorkspace _currentWorkspace;
        private readonly Updater _updater;

        public TextDocumentController(CurrentWorkspace currentWorkspace, Updater updater)
        {
            _currentWorkspace = currentWorkspace;
            _updater = updater;
        }

        [JsonRpcFunction("textDocument/didOpen", deserializeParamterIntoSingleObject: false)]
        public async Task DidOpen(RpcTextDocumentItem textDocument)
        {
            _currentWorkspace.OpenFile(textDocument.Uri, textDocument.Text);
            await _updater.Update();
        }

        [JsonRpcFunction("textDocument/didClose", deserializeParamterIntoSingleObject: false)]
        public async Task DidClose(RpcTextDocumentIdentifier textDocument)
        {
            await _currentWorkspace.CloseFile(textDocument.Uri);
            await _updater.Update();
        }

        [JsonRpcFunction("textDocument/didChange", deserializeParamterIntoSingleObject: false)]
        public async Task DidChange(RpcVersionedTextDocumentIdentifier textDocument, ImmutableArray<RpcTextDocumentContentChangeEvent> contentChanges)
        {
            _currentWorkspace.ChangeFile(textDocument.Uri, contentChanges);
            await _updater.Update();
        }
    }
}
