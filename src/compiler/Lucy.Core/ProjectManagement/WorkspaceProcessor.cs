using Lucy.Core.Parser;
using Lucy.Core.SemanticAnalysis;
using System.Collections.Generic;

namespace Lucy.Core.ProjectManagement
{
    public class WorkspaceProcessor
    {
        private readonly Workspace _workspace;
        private readonly Dictionary<string, Document> _processed = new();

        public WorkspaceProcessor(Workspace workspace)
        {
            _workspace = workspace;
            Update();
        }

        public void Update()
        {
            UpdateDocuments();
            UpdateSyntaxTree();
            UpdateSemanticAnalysis();
        }


        private void UpdateDocuments()
        {
            var toRemove = new List<string>();
            foreach (var processedDocumentPath in _processed.Keys)
            {
                if (!_workspace.ContainsFile(processedDocumentPath))
                    toRemove.Add(processedDocumentPath);
            }

            foreach (var tr in toRemove)
                _processed.Remove(tr);

            foreach (var textDocument in _workspace.Documents)
            {
                if (_processed.ContainsKey(textDocument.Path))
                    continue;

                _processed.Add(textDocument.Path, new Document(textDocument));
            }
        }

        public IEnumerable<Document> Documents => _processed.Values;

        private void UpdateSyntaxTree()
        {
            foreach(var doc  in _processed.Values)
            {
                if (doc.SyntaxTree == null || doc.SyntaxTree.Version != doc.Text.Version)
                    doc.SyntaxTree = CodeParser.Parse(doc.Text);
            }
        }

        private void UpdateSemanticAnalysis()
        {
            foreach (var doc in _processed.Values)
            {
                if (doc.SyntaxTree != null)
                    SemanticAnalyzer.Run(doc.SyntaxTree);
            }
        }
    }

    public class Document
    {
        public Document(TextDocument text)
        {
            Text = text;
        }

        public string Path => Text.Path;
        public TextDocument Text { get; set; }
        public DocumentSyntaxNode? SyntaxTree { get; set; }
    }
}
