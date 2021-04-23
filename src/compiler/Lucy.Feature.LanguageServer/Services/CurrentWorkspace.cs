using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;
using Lucy.Core.Model;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service]
    public class CurrentWorkspace
    {
        private Workspace? _workspace;
        private WorkspaceProcessor? _workspaceProcessor;
        private HashSet<string> _openFiles = new HashSet<string>();
        private readonly CurrentRpcConnection _currentRpcConnection;
        private readonly RpcMapper _rpcMapper;

        public CurrentWorkspace(CurrentRpcConnection currentRpcConnection, RpcMapper rpcMapper)
        {
            _currentRpcConnection = currentRpcConnection;
            _rpcMapper = rpcMapper;
        }

        public async Task Init(Uri uri)
        {
            _openFiles = new HashSet<string>();
            _rpcMapper.SetWorkspaceRootUri(uri);
            _workspace = await Workspace.CreateFromPath(_rpcMapper.ToSystemPath(uri));
            _workspaceProcessor = new WorkspaceProcessor(_workspace);
        }

        public void Update()
        {
            if (_workspaceProcessor == null)
                throw new Exception("Workspace is not initialized");

            _workspaceProcessor.Update();
        }

        public void ChangeFile(Uri uri, ImmutableArray<RpcTextDocumentContentChangeEvent> contentChanges)
        {
            foreach(var change in contentChanges)
            {
                if (change.Range == null)
                    Workspace.AddOrUpdateDocument(new TextDocument(_rpcMapper.ToWorkspacePath(uri), change.Text));
                else
                {
                    var start = new Position2D(change.Range.Start.Line, change.Range.Start.Character);
                    var end = new Position2D(change.Range.End.Line, change.Range.End.Character);
                    Workspace.Change(_rpcMapper.ToWorkspacePath(uri), new Range2D(start, end), change.Text);
                }
            }
        }

        public void OpenFile(Uri uri, string text)
        {
            _openFiles.Add(_rpcMapper.ToWorkspacePath(uri));
            Workspace.AddOrUpdateDocument(new TextDocument(_rpcMapper.ToWorkspacePath(uri), text));
        }

        public async Task CloseFile(Uri uri)
        {
            _openFiles.Remove(_rpcMapper.ToWorkspacePath(uri));
            var content = await File.ReadAllTextAsync(_rpcMapper.ToSystemPath(uri));
            Workspace.AddOrUpdateDocument(new TextDocument(_rpcMapper.ToWorkspacePath(uri), content));
        }

        Workspace Workspace => _workspace ?? throw new Exception("Workspace was not initilized");
    }

    public class RpcPublishDiagnosticsParams
    {
        /// <summary>
        /// The URI for which diagnostic information is reported.
        /// </summary>
        public Uri Uri { get; set; } = null!;

        /// <summary>
        /// An array of diagnostic information items.
        /// </summary>
        public RpcDiagnostic[] Diagnostics { get; set; } = Array.Empty<RpcDiagnostic>();
    }
}
