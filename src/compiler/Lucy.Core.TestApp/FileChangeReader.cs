using Lucy.Core.ProjectManagement;

namespace Lucy.Core.TestApp;

internal class TestCaseReader
{
    private readonly Workspace _workspace;
    private readonly Dictionary<int, List<(string Path, string Content)>> _versions;
    private int _currentVersion;

    public TestCaseReader(Workspace workspace, string dir)
    {
        dir = Path.GetFullPath(dir);
        var files = Directory.GetFiles(dir, "*.lucy", SearchOption.AllDirectories);
        _versions = new Dictionary<int, List<(string Path, string Content)>>();
        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var workspacePath = file[dir.Length..].Replace("\\", "/");

            var version = 1;
            var underscorePos = workspacePath.LastIndexOf("_", StringComparison.InvariantCulture);
            if (underscorePos != -1)
            {
                var dotPos = workspacePath.IndexOf('.', underscorePos + 1);
                var versionStr = workspacePath.Substring(underscorePos + 1, dotPos - underscorePos - 1);
                version = int.Parse(versionStr);
                workspacePath = workspacePath.Remove(underscorePos, dotPos - underscorePos);
            }

            if (!_versions.TryGetValue(version, out var list))
            {
                list = new List<(string, string)>();
                _versions[version] = list;
            }

            list.Add((workspacePath, content));
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
            Console.WriteLine("Applying " + file.Path + " v" + _currentVersion);
            if (_workspace.ContainsFile(file.Path))
                _workspace.UpdateFile(WorkspaceDocument.Create(file.Path, file.Content));
            else
                _workspace.AddDocument(WorkspaceDocument.Create(file.Path, file.Content));
        }
        return true;
    }
}
