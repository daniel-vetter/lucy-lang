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
        public string Content { get; }
    }
}
