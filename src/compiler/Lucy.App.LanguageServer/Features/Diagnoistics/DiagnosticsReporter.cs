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
                .GroupBy(x => x.DocumentPath)
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
                        var range2D = _currentWorkspace.ToRange2D(documentWithError.DocumentPath, x.Range);

                        return new RpcDiagnostic
                        {
                            Range = range2D.ToRpcRange(),
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
