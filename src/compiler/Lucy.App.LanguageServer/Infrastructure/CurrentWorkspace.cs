using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;
using System;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Infrastructure
{
    [Service(Lifetime.Singleton)]
    public class CurrentWorkspace
    {
        private readonly IFileSystem _fileSystem;

        private Workspace? _workspace;
        private SystemPath? _rootPath;

        public CurrentWorkspace(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task Load(SystemPath path)
        {
            if (_workspace != null)
                throw new InvalidOperationException("A workspace is already loaded");

            var ws = new Workspace();
            var rootPathLength = path.ToString().Length;
            var files = await _fileSystem.GetFilesInDirectory(path);
            
            foreach(var file in files)
                ws.AddFile(file.ToString().Substring(rootPathLength).Replace("\\", "/"), await _fileSystem.ReadAllText(file));

            _workspace = ws;
            _rootPath = path;
        }

        public void AddOrUpdate(SystemPath path, string content)
        {
            if (_workspace == null)
                throw new InvalidOperationException("No workspace was loaded");

            var workspacePath = ToWorkspacePath(path);

            if (_workspace.ContainsFile(workspacePath))
                _workspace.UpdateFile(workspacePath, content);
            else
                _workspace.AddFile(ToWorkspacePath(path), content);
        }

        public void IncrementelUpdate(SystemPath path, Range2D range, string content)
        {
            if (_workspace == null)
                throw new InvalidOperationException("No workspace was loaded");

            var workspacePath = ToWorkspacePath(path);

            _workspace.UpdateFile(workspacePath, range, content);
        }

        public string ToWorkspacePath(SystemPath systemPath)
        {
            if (_rootPath == null)
                throw new InvalidOperationException("No workspace was loaded");

            return systemPath.ToString().Substring(_rootPath.ToString().Length).Replace("\\", "/");
        }

        public SystemPath ToSystemPath(string workspacePath)
        {
            if (_rootPath == null)
                throw new InvalidOperationException("No workspace was loaded");

            return _rootPath.Append(workspacePath);
        }
    }
}
