using Lucy.Core.ProjectManagement;
using System;

namespace Lucy.Core.SemanticAnalysis
{
    public class SemanticAnalyzer
    {
        public static void Run(Workspace workspace)
        {
            foreach(var document in workspace.Documents)
            {
                var rootNode = document.SyntaxTree;
                if (rootNode == null)
                    throw new Exception($"Could not find a syntax tree for workspace document  '{document.Path}'.");

                ParentAssigner.Run(rootNode);
                ScopeAssigner.Run(rootNode);
                TypeDiscovery.Run(rootNode);
                FunctionSymbolResolver.Run(rootNode);
            }
        }
    }
}
