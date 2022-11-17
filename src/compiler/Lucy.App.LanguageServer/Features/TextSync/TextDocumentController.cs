using System.Collections.Immutable;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using System;

namespace Lucy.Feature.LanguageServer.RpcController
{
    [Service(Lifetime.Singleton)]
    internal class TextDocumentController
    {
        private readonly CurrentWorkspace _currentWorkspace;
        //private readonly DiagnosticsReporter _diagnosticsReporter;

        public TextDocumentController(CurrentWorkspace currentWorkspace/*, DiagnosticsReporter diagnosticsReporter*/)
        {
            _currentWorkspace = currentWorkspace;
            //_diagnosticsReporter = diagnosticsReporter;
        }

        [JsonRpcFunction("textDocument/didOpen", deserializeParamterIntoSingleObject: false)]
        public Task DidOpen(RpcTextDocumentItem textDocument)
        {
            if (_currentWorkspace.Workspace == null)
                throw new Exception("No workspace loaded.");

            _currentWorkspace.AddOrUpdate(textDocument.Uri, textDocument.Text);
            //await _diagnosticsReporter.Report();
            return Task.CompletedTask;
        }

        [JsonRpcFunction("textDocument/didClose", deserializeParamterIntoSingleObject: false)]
        public Task DidClose(RpcTextDocumentIdentifier textDocument)
        {
            return Task.CompletedTask;
        }

        [JsonRpcFunction("textDocument/didChange", deserializeParamterIntoSingleObject: false)]
        public Task DidChange(RpcVersionedTextDocumentIdentifier textDocument, ImmutableArray<RpcTextDocumentContentChangeEvent> contentChanges)
        {
            foreach(var change in contentChanges)
            {
                if (change.Range != null)
                    throw new Exception("Incremental changes currently not supported.");

                _currentWorkspace.AddOrUpdate(textDocument.Uri, change.Text);
            }

            //await _diagnosticsReporter.Report();
            return Task.CompletedTask;
        }
    }
}
