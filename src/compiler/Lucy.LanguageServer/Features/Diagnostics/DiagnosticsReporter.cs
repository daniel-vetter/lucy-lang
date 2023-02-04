using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lucy.App.LanguageServer.Infrastructure;
using Lucy.App.LanguageServer.Models;
using Lucy.Common;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using Lucy.Infrastructure.RpcServer;

namespace Lucy.App.LanguageServer.Features.Diagnostics;

[Service(Lifetime.Singleton)]
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
        Profiler.Start("Reporting errors");

        var errors = _currentWorkspace.Analysis.Get<AllErrorsCollector>().GetAllErrors();

        var documentsToReport = errors
            .GroupBy(x => x.DocumentPath)
            .Select(x => new ReportJob(x.Key, x.ToArray()))
            .ToList();

        var documents = documentsToReport
            .Select(x => x.DocumentPath)
            .ToHashSet();

        foreach(var document in _documentsWithErrorsInLastReport.Except(documents))
        {
            documentsToReport.Add(new ReportJob(document, Array.Empty<ErrorWithRange>()));
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

        Profiler.End("Reporting errors");
    }

    private record ReportJob(string DocumentPath, ErrorWithRange[] Errors);
}