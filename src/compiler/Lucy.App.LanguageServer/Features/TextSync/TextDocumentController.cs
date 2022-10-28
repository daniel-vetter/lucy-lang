using System.Collections.Immutable;
using System.Threading.Tasks;
using Lucy.Infrastructure.RpcServer;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.App.LanguageServer.Infrastructure;
using System;
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
            if (_currentWorkspace.Workspace == null)
                throw new Exception("No workspace loaded.");

            _currentWorkspace.Workspace.AddOrUpdateDocument(_currentWorkspace.ToWorkspacePath(textDocument.Uri), textDocument.Text);
            _currentWorkspace.Process();
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
            if (_currentWorkspace.Workspace == null)
                throw new Exception("No workspace loaded.");

            var document = _currentWorkspace.Workspace.Get(_currentWorkspace.ToWorkspacePath(textDocument.Uri));
            if (document == null)
                throw new Exception("Could not find document: " + textDocument.Uri);

            var fileContent = document.SourceCode;
            foreach(var change in contentChanges)
            {
                if (change.Range == null)
                {
                    fileContent = change.Text;
                }
                else
                {
                    var startIndex = -1;
                    var endIndex = -1;
                    var curLine = 0;
                    var curChar = 0;
                    for (int i = 0; true; i++)
                    {
                        if (curLine == change.Range.Start.Line && curChar == change.Range.Start.Character)
                            startIndex = i;

                        if (curLine == change.Range.End.Line && curChar == change.Range.End.Character)
                        {
                            endIndex = i;
                            break;
                        }

                        if (i == fileContent.Length)
                            break;
                        
                        if (fileContent[i] == '\n')
                        {
                            curLine++;
                            curChar = 0;
                        }
                        else
                        {
                            curChar++;
                        }
                    }                  

                    fileContent = fileContent.Substring(0, startIndex) + change.Text + fileContent.Substring(endIndex);
                }
            }

            _currentWorkspace.Workspace.AddOrUpdateDocument(_currentWorkspace.ToWorkspacePath(textDocument.Uri), fileContent);
            _currentWorkspace.Process();
            await _diagnosticsReporter.Report();
        }
    }
}
