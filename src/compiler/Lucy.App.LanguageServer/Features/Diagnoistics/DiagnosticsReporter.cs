using Lucy.App.LanguageServer.Infrastructure;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.SemanticAnalysis.Handler;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Infrastructure.RpcServer;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Features.Diagnoistics
{
    [Service]
    public class DiagnosticsReporter
    {
        private readonly CurrentWorkspace _currentWorkspace;
        private readonly JsonRpcServer _jsonRpcServer;
        private HashSet<string> _documentsWithErrorsInLastReport = new();


        public DiagnosticsReporter(CurrentWorkspace currentWorkspace, JsonRpcServer jsonRpcServer)
        {
            _currentWorkspace = currentWorkspace;
            _jsonRpcServer = jsonRpcServer;
        }

        public async Task Report()
        {
            var errors = _currentWorkspace.Analysis.GetAllErrors();

            var documentsToReport = errors
                .GroupBy(x => x.NodeId.DocumentPath)
                .Select(x => new ReportJob(x.Key, x.ToArray()))
                .ToList();

            var documents = documentsToReport
                .Select(x => x.DocumentPath)
                .ToHashSet();

            foreach(var document in _documentsWithErrorsInLastReport.Except(documents))
            {
                documentsToReport.Add(new ReportJob(document, Array.Empty<Error>()));
            }

            _documentsWithErrorsInLastReport = documents;

            foreach (var documentWithError in documentsToReport)
            {
                await _jsonRpcServer.SendNotification("textDocument/publishDiagnostics", new RpcPublishDiagnosticsParams
                {
                    Diagnostics = documentWithError.Errors.Select(x =>
                    {
                        var node = _currentWorkspace.Analysis.GetNodeById(x.NodeId);
                        var range1D = _currentWorkspace.Analysis.GetRangeFromNode(node);

                        var range2D = _currentWorkspace.ToRange2D(documentWithError.DocumentPath, range1D);

                        return new RpcDiagnostic
                        {
                            Range = range2D.ToRpcRange(),
                            Code = "01",
                            Severity = RpcDiagnosticSeverity.Error,
                            Message = x.Message
                        };
                    }).ToArray(),
                    Uri = _currentWorkspace.ToSystemPath(documentWithError.DocumentPath)
                });
            }
        }

        private record ReportJob(string DocumentPath, Error[] Errors);
    }
}
