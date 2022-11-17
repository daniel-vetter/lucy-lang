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

        public Workspace? Workspace { get; private set; }
        public SystemPath? RootPath { get; private set; }

        public CurrentWorkspace(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task Load(SystemPath path)
        {
            var ws = new Workspace();
            var rootPathLength = path.ToString().Length;
            var files = await _fileSystem.GetFilesInDirectory(path);
            
            foreach(var file in files)
                ws.AddOrUpdateFile(file.ToString().Substring(rootPathLength).Replace("\\", "/"), await _fileSystem.ReadAllText(file));

            Workspace = ws;
            RootPath = path;
        }

        public void AddOrUpdate(SystemPath path, string content)
        {
            Workspace.AddOrUpdateFile(ToWorkspacePath(path), content);
        }

        public string ToWorkspacePath(SystemPath systemPath)
        {
            if (RootPath == null)
                throw new Exception("No workspace path availible.");

            return systemPath.ToString().Substring(RootPath.ToString().Length).Replace("\\", "/");
        }

        public SystemPath ToSystemPath(string workspacePath)
        {
            if (RootPath == null)
                throw new Exception("No workspace path availible.");

            return RootPath.Append(workspacePath);
        }
    }
}
