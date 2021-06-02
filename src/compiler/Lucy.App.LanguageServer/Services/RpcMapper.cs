using System;
using Lucy.Common.ServiceDiscovery;

namespace Lucy.Feature.LanguageServer.Services
{
    [Service(Lifetime.Singleton)]
    public class RpcMapper
    {
        private Uri? _rootUri;

        public void SetWorkspaceRootUri(Uri rootUri)
        {
            _rootUri = rootUri;
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
