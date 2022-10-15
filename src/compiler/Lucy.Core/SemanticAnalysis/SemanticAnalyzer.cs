using Lucy.Core.Model;
using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using System;
using System.Collections.Generic;

namespace Lucy.Core.SemanticAnalysis
{
    public class SemanticModelGenerator
    {
        public static SemanticModel Run(SyntaxTreeNode syntaxTreeNode)
        {
            var semanticModel = new SemanticModel();
            ScopeAssigner.Run(syntaxTreeNode, semanticModel);
            TypeDiscovery.Run(syntaxTreeNode, semanticModel);
            EntryPointFinder.Run(syntaxTreeNode, semanticModel);
            FunctionSymbolResolver.Run(syntaxTreeNode, semanticModel);
            return semanticModel;
        }
    }

    public class SemanticModel
    {
        private Dictionary<SyntaxTreeNode, Scope> _scopes = new();
        private Dictionary<SyntaxTreeNode, FunctionInfo> _functionInfos = new();
        private Dictionary<SyntaxTreeNode, List<Issue>> _issues = new();
        
        public void SetScope(SyntaxTreeNode node, Scope scope)
        {
            _scopes.Add(node, scope);
        }

        public Scope GetScope(SyntaxTreeNode node)
        {
            if (!_scopes.TryGetValue(node, out var scope))
                throw new Exception("No scope for the given node available.");
            return scope;
        }

        public void SetFunctionInfo(FunctionDeclarationStatementSyntaxNode functionDeclarationStatementSyntaxNode, FunctionInfo functionInfo)
        {
            _functionInfos.Add(functionDeclarationStatementSyntaxNode, functionInfo);
        }

        public void SetFunctionInfo(FunctionCallExpressionSyntaxNode functionCallExpressionSyntaxNode, FunctionInfo functionInfo)
        {
            _functionInfos.Add(functionCallExpressionSyntaxNode, functionInfo);
        }

        public FunctionInfo GetFunctionInfo(FunctionDeclarationStatementSyntaxNode functionDeclarationStatementSyntaxNode)
        {
            if (!_functionInfos.TryGetValue(functionDeclarationStatementSyntaxNode, out var functionInfo))
                throw new Exception("No function info for the given function declaration available.");
            return functionInfo;
        }

        public FunctionInfo GetFunctionInfo(FunctionCallExpressionSyntaxNode functionCallExpressionSyntaxNode)
        {
            if (!_functionInfos.TryGetValue(functionCallExpressionSyntaxNode, out var functionInfo))
                throw new Exception("No function info for the given function call available.");
            return functionInfo;
        }

        public void AddErrorIssue(SyntaxTreeNode node, string message) => AddIssue(node, IssueSeverity.Error, message);
        public void AddWarningIssue(SyntaxTreeNode node, string message) => AddIssue(node, IssueSeverity.Warning, message);

        public void AddIssue(SyntaxTreeNode node, IssueSeverity severity, string message)
        {
            if (!_issues.TryGetValue(node, out var list))
            {
                list = new List<Issue>();
                _issues.Add(node, list);
            }
            list.Add(new Issue(severity, message));
        }

        public IEnumerable<Issue> GetIssues(SyntaxTreeNode node)
        {
            if (_issues.TryGetValue(node, out var list))
                return list;
            return Array.Empty<Issue>();
        }
    }
}
