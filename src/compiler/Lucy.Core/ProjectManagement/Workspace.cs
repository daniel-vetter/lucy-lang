using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Lucy.Core.ProjectManagement
{
    public class WorkspaceDocument
    {
        public WorkspaceDocument(string path, string content)
        {
            Path = path;
            Content = content;
        }

        public string Path { get; }
        public string Content { get; private set; }
        public DocumentSyntaxNode? SyntaxTree { get; set; }
        public SemanticModel? SemanticModel { get; set; }
    }

    public class Workspace
    {
        Dictionary<string, WorkspaceDocument> _documents = new Dictionary<string, WorkspaceDocument>();

        public static async Task<Workspace> CreateFromPath(string path)
        {
            var files = Directory.GetFiles(path, "*.lucy", SearchOption.AllDirectories);
            var tasks = files.Select(async x => new
            {
                Path = x,
                Content = await File.ReadAllTextAsync(x)
            });

            var ws = new Workspace();
            foreach (var file in await Task.WhenAll(tasks))
            {
                var subPath = file.Path.Substring(path.Length).Replace("\\", "/");
                ws.AddDocument(subPath, file.Content);
            }
            return ws;
        }

        public void Process()
        {
            foreach(var doc in Documents)
            {
                doc.SyntaxTree = Parser.Parse(doc.Content);
                doc.SemanticModel = SemanticModelGenerator.Run(doc.SyntaxTree);
            }
        }

        public void AddDocument(string path, string content) => _documents.Add(path, new WorkspaceDocument(path, content));
        public void AddOrUpdateDocument(string path, string content) => _documents[path] = new WorkspaceDocument(path, content);
        public void RemoveDocument(string path) => _documents.Remove(path);
        public bool ContainsFile(string path) => _documents.ContainsKey(path);

        public WorkspaceDocument? Get(string path)
        {
            if (_documents.TryGetValue(path, out var document))
                return document;
            return null;
        }

        public IEnumerable<WorkspaceDocument> Documents => _documents.Values;
    }
}
