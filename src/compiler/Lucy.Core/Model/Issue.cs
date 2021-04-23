namespace Lucy.Core.Model
{
    public class Issue
    {
        public Issue(IssueSeverity severity, string message)
        {
            Severity = severity;
            Message = message;
        }

        public IssueSeverity Severity { get; }
        public string Message { get; }
    }

    public enum IssueSeverity
    {
        Warning,
        Error
    }
}
