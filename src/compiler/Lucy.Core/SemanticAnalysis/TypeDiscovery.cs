using Lucy.Core.Helper;
using Lucy.Core.Parsing.Nodes.Statements.FunctionDeclaration;
using Lucy.Core.Parsing;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis
{
    //Requires: Scopes
    internal class TypeDiscovery
    {
        internal static void Run(SyntaxTreeNode node, SemanticAnalyzer semanticModel)
        {
            /*
            if (node is FunctionDeclarationStatementSyntaxNode functionDeclarationStatementSyntaxNode)
            {
                var functionInfo = new FunctionInfo(Guid.NewGuid().ToString(), functionDeclarationStatementSyntaxNode);
                semanticModel.SetFunctionInfo(functionDeclarationStatementSyntaxNode, functionInfo);
                semanticModel.GetScope(node).AddSymbol(functionInfo);
                //semanticModel.AddExportedFunction(functionInfo);
            }

            foreach (var child in node.GetChildNodes())
                Run(child, semanticModel);
            */
        }
    }

    public class FunctionInfo : Symbol
    {
        public FunctionInfo(string id, FunctionDeclarationStatementSyntaxNode declaration) : base(declaration.FunctionName.Token.Text)
        {
            Id = id;
            Declaration = declaration;
            Parameter = declaration.ParameterList.Select(x => new FunctionParameterInfo(x.VariableDeclaration.VariableName.Token.Text)).ToImmutableArray();

            if (declaration.Modifiers.Any(x => x.Token.Text == "cdecl"))
                CallingConvention = CallingConvention.Cdecl;
            else if (declaration.Modifiers.Any(x => x.Token.Text == "stdcall"))
                CallingConvention = CallingConvention.Stdcall;
            else CallingConvention = CallingConvention.Internal;

            if (declaration.ExternLibraryName != null)
                Extern = new FunctionInfoExtern(declaration.ExternLibraryName.Value, declaration.ExternFunctionName?.Value ?? declaration.FunctionName.Token.Text);
        }

        public string Id { get; }
        public ImmutableArray<FunctionParameterInfo> Parameter { get; }
        public FunctionDeclarationStatementSyntaxNode Declaration { get; }
        public FunctionInfoExtern? Extern { get; }
        public CallingConvention CallingConvention { get; }
        public bool IsEntryPoint { get; set; }
    }

    public enum CallingConvention
    {
        Cdecl,
        Stdcall,
        Internal
    }

    public record FunctionInfoExtern(string LibraryName, string FunctionName);
    public record FunctionParameterInfo(string Name);

}
