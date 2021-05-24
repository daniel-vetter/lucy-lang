using System.Threading.Tasks;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service]
    public class DiagnosticsUpdater
    {
        private readonly CurrentRpcConnection _currentRpcConnection;
        private readonly RpcMapper _rpcMapper;

        public DiagnosticsUpdater(CurrentRpcConnection currentRpcConnection, RpcMapper rpcMapper)
        {
            _currentRpcConnection = currentRpcConnection;
            _rpcMapper = rpcMapper;
        }

        internal async Task Update(WorkspaceProcessor workspaceProcessor)
        {
            /*
            foreach (var addedOrUpdated in workspaceProcessor.Documents)
            {
                var diagnostics = new List<RpcDiagnostic>();
                foreach (var logEntry in addedOrUpdated.ParserResult.LogEntries)
                {
                    diagnostics.Add(new RpcDiagnostic
                    {
                        Message = logEntry.Message,
                        Range = _rpcMapper.ConvertRange(addedOrUpdated.TextDocument.ConvertRange(logEntry.Range))
                    });
                }

                await Publish(addedOrUpdated.Path, diagnostics.ToArray());
            }
            */
        }

        private async Task Publish(string documentWorkspacePath, RpcDiagnostic[] diagnostics)
        {
            await _currentRpcConnection.SendNotification("textDocument/publishDiagnostics", new RpcPublishDiagnosticsParams
            {
                Uri = _rpcMapper.WorkspacePathToRpcPath(documentWorkspacePath),
                Diagnostics = diagnostics
            });
        }
    }
}
