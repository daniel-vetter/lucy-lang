using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Statements.FunctionDeclaration;
using System;

namespace Lucy.Core.SemanticAnalysis
{
    //Requires: Scopes
    internal class TypeDiscovery
    {
        internal static void Run(SyntaxNode node)
        {
            if (node is FunctionDeclarationStatementSyntaxNode functionDeclarationStatementSyntaxNode)
            {
                var functionInfo = new FunctionInfo(Guid.NewGuid().ToString(), functionDeclarationStatementSyntaxNode);
                node.SetAnnotation(functionInfo);
                node.GetScope().AddSymbol(functionInfo);
            }

            foreach (var child in node.GetChildNodes())
                Run(child);
        }
    }

    public static class TypeDiscoveryExtensionMethods
    {
        public static FunctionInfo GetFunctionInfo(this FunctionDeclarationStatementSyntaxNode node) => node.GetRequiredAnnotation<FunctionInfo>();
    }

    public class FunctionInfo : Symbol
    {
        public FunctionInfo(string id, FunctionDeclarationStatementSyntaxNode declaration) : base(declaration.FunctionName.Value)
        {
            Id = id;
            Declaration = declaration;
            if (declaration.ExternLibraryName != null)
                Extern = new FunctionInfoExtern(declaration.ExternLibraryName.Value, declaration.ExternFunctionName?.Value ?? declaration.FunctionName.Value);
        }

        public string Id { get; }
        public FunctionDeclarationStatementSyntaxNode Declaration { get; }
        public FunctionInfoExtern? Extern { get; }
        public bool IsEntryPoint { get; set; }
    }

    public record FunctionInfoExtern(string LibraryName, string FunctionName);

}
