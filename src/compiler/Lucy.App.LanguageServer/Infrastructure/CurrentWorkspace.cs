using Lucy.Common.ServiceDiscovery;
using Lucy.Core.ProjectManagement;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Lucy.App.LanguageServer.Infrastructure
{
    [Service(Lifetime.Singleton)]
    public class CurrentWorkspace
    {
        private readonly IFileSystem _fileSystem;

        private Workspace? _workspace;
        private SemanticDatabase? _semanticDatabase;
        private SystemPath? _rootPath;

        public CurrentWorkspace(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public IDb Analysis => _semanticDatabase ?? throw new Exception("No workspace was loaded");

        public async Task Load(SystemPath path)
        {
            if (_workspace != null)
                throw new InvalidOperationException("A workspace is already loaded");

            var ws = new Workspace();
            var rootPathLength = path.ToString().Length;
            var files = await _fileSystem.GetFilesInDirectory(path);
            
            foreach(var file in files)
                ws.AddFile(file.ToString().Substring(rootPathLength).Replace("\\", "/"), await _fileSystem.ReadAllText(file));

            _semanticDatabase = new SemanticDatabase(ws, "C:\\language-server-output");
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

        public Position1D ToPosition1D(SystemPath systemPath, Position2D position2D) => ToPosition1D(ToWorkspacePath(systemPath), position2D);
        public Position1D ToPosition1D(string documentPath, Position2D position2D)
        {
            if (_workspace == null)
                throw new InvalidOperationException("No workspace was loaded");

            return _workspace.GetFile(documentPath).ConvertTo1D(position2D);
        }

        public Position2D ToPosition2D(SystemPath systemPath, Position1D position1D) => ToPosition2D(ToWorkspacePath(systemPath), position1D);
        public Position2D ToPosition2D(string documentPath, Position1D position1D)
        {
            if (_workspace == null)
                throw new InvalidOperationException("No workspace was loaded");

            return _workspace.GetFile(documentPath).ConvertTo2D(position1D);
        }

        public Range2D ToRange2D(SystemPath systemPath, Range1D range1D) => ToRange2D(ToWorkspacePath(systemPath), range1D);
        public Range2D ToRange2D(string documentPath, Range1D range1D)
        {
            if (_workspace == null)
                throw new InvalidOperationException("No workspace was loaded");

            return _workspace.GetFile(documentPath).ConvertTo2D(range1D);
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
