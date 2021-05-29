using System;
using Lucy.Feature.LanguageServer.Models;
using Lucy.Common.ServiceDiscovery;
using Lucy.Core.Model;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service]
    public class RpcMapper
    {
        private Uri? _rootUri;

        public void SetWorkspaceRootUri(Uri rootUri)
        {
            _rootUri = rootUri;
        }


        public RpcRange ConvertRange(Range2D range)
        {
            return new RpcRange
            {
                Start = new RpcPosition { Line = range.Start.Line, Character = range.Start.Column },
                End = new RpcPosition { Line = range.End.Line, Character = range.End.Column },
            };
        }

        public string ToSystemPath(Uri uri)
        {
            return uri.LocalPath.Substring(1);
        }

        public Uri WorkspacePathToRpcPath(string workspacePath)
        {
            if (_rootUri == null)
                throw new Exception("Workspace not initialized");

            return new Uri(_rootUri.ToString() + workspacePath);
        }

        public string ToWorkspacePath(Uri uri)
        {
            var rootPath = ToSystemPath(_rootUri ?? throw new Exception("Workspace was not initialized"));
            var path = ToSystemPath(uri);
            return path.Substring(rootPath.Length);
        }
    }
}
