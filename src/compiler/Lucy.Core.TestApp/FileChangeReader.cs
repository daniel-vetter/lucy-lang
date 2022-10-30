using Lucy.Core.Parsing;
using Lucy.Core.ProjectManagement;
using System.Runtime.CompilerServices;

namespace Lucy.Core.TestApp;

internal class FileChangeReader
{
    private readonly Workspace _workspace;
    private Dictionary<int, List<CodeFile>> _versions;
    private int _currentVersion = 0;

    public FileChangeReader(Workspace workspace, string dir)
    {
        dir = Path.GetFullPath(dir);
        var files = Directory.GetFiles(dir, "*.lucy", SearchOption.AllDirectories);
        _versions = new Dictionary<int, List<CodeFile>>();
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var workspacePath = file.Substring(dir.Length).Replace("\\", "/");

            var version = 1;
            var underscorePos = workspacePath.LastIndexOf("_");
            if (underscorePos != -1)
            {
                var dotPos = workspacePath.IndexOf('.', underscorePos + 1);
                var versionStr = workspacePath.Substring(underscorePos + 1, dotPos - underscorePos - 1);
                version = int.Parse(versionStr);
                workspacePath = workspacePath.Remove(underscorePos, dotPos - underscorePos);
            }

            if (!_versions.TryGetValue(version, out var list))
            {
                list = new List<CodeFile>();
                _versions[version] = list;
            }

            list.Add(new CodeFile(workspacePath, content, Parser.Parse(workspacePath, content)));
        }
        _workspace = workspace;
    }

    public bool NextVersion()
    {
        _currentVersion++;

        if (!_versions.TryGetValue(_currentVersion, out var files))
            return false;

        foreach (var file in files)
        {
            Console.WriteLine("Appling " + file.Path + " v" + _currentVersion);
            _workspace.AddOrUpdateDocument(file);
        }
        return true;
    }
}
