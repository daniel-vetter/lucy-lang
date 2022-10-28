using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using Lucy.Core.ProjectManagement;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Feature.LanguageServer.Services;
using Lucy.Infrastructure.RpcServer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Features.Diagnoistics
{
    [Service]
    public class DiagnosticsReporter
    {
        private readonly CurrentWorkspace _currentWorkspace;
        private readonly JsonRpcServer _jsonRpcServer;

        public DiagnosticsReporter(CurrentWorkspace currentWorkspace, JsonRpcServer jsonRpcServer)
        {
            _currentWorkspace = currentWorkspace;
            _jsonRpcServer = jsonRpcServer;
        }

        public async Task Report()
        {
            if (_currentWorkspace.RootPath == null || _currentWorkspace.Workspace == null)
                return;

            foreach (var doc in _currentWorkspace.Workspace.Documents)
            {
                var issues = GetIssues(doc);

                await _jsonRpcServer.SendNotification("textDocument/publishDiagnostics", new RpcPublishDiagnosticsParams
                {
                    Diagnostics = issues,
                    Uri = _currentWorkspace.ToSystemPath(doc.Path)
                });
            }
        }

        private RpcDiagnostic[] GetIssues(CodeFile doc)
        {
            List<RpcDiagnostic> result = new();
            void Walk(SyntaxTreeNode node)
            {
                foreach (var child in node.GetChildNodes())
                {
                    if (child.Source is Syntetic source)
                    {
                        var range = child.GetRange();
                        result.Add(new RpcDiagnostic
                        {
                            Range = new RpcRange
                            {
                                Start = new RpcPosition { Line = range.Start.Line, Character = range.Start.Column },
                                End = new RpcPosition { Line = range.End.Line, Character = range.End.Column },
                            },
                            Code = "M",
                            Message = source.ErrorMessage ?? "<missing errror message on node " + child.GetType().Name + ">",
                            Severity = RpcDiagnosticSeverity.Error,
                            Source = "lucy",
                        });
                    }
                    else
                    {
                        Walk(child);
                    }
                }
            }

            if (doc.SyntaxTree != null)
                Walk(doc.SyntaxTree);
            return result.ToArray();
        }
    }
}
