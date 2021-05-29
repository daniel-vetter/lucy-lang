using System.Collections.Immutable;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service]
    internal class TextDocumentController
    {
        [JsonRpcFunction("textDocument/didOpen", deserializeParamterIntoSingleObject: false)]
        public Task DidOpen(RpcTextDocumentItem textDocument)
        {
            //_currentWorkspace.OpenFile(textDocument.Uri, textDocument.Text);
            return Task.CompletedTask;
        }

        [JsonRpcFunction("textDocument/didClose", deserializeParamterIntoSingleObject: false)]
        public Task DidClose(RpcTextDocumentIdentifier textDocument)
        {
            //await _currentWorkspace.CloseFile(textDocument.Uri);
            return Task.CompletedTask;
        }

        [JsonRpcFunction("textDocument/didChange", deserializeParamterIntoSingleObject: false)]
        public Task DidChange(RpcVersionedTextDocumentIdentifier textDocument, ImmutableArray<RpcTextDocumentContentChangeEvent> contentChanges)
        {
            //_currentWorkspace.ChangeFile(textDocument.Uri, contentChanges);
            return Task.CompletedTask;
        }
    }
}
