using Lucy.Core.Model;
using Lucy.Core.Parsing;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucy.Core.SemanticAnalysis
{
    public static class IssueExtensions
    {
        public static void AddError(this SyntaxTreeNode node, string message) => node.AddIssue(new Issue(IssueSeverity.Error, message));
        public static void AddWarning(this SyntaxTreeNode node, string message) => node.AddIssue(new Issue(IssueSeverity.Warning, message));

        private static void AddIssue(this SyntaxTreeNode node, Issue issue)
        {
            var issueList = node.GetAnnotation<List<Issue>>();
            if (issueList == null)
            {
                issueList = new List<Issue>();
                node.SetAnnotation(issueList);
            }

            issueList.Add(issue);
        }

        public static ImmutableArray<Issue> GetIssues(this SyntaxTreeNode node)
        {
            var issueList = node.GetAnnotation<List<Issue>>();
            if (issueList == null || issueList.Count == 0)
                return ImmutableArray<Issue>.Empty;

            return issueList.ToImmutableArray();
        }
    }
}
