using Lucy.Core.Helper;
using Lucy.Core.Parsing;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucy.Core.SemanticAnalysis
{
    internal class ScopeAssigner
    {
        internal static void Run(SyntaxTreeNode node, SemanticAnalyzer semanticModel)
        {
            var rootScope = new Scope(null);
            Traverse(node, rootScope, semanticModel);
        }

        private static void Traverse(SyntaxTreeNode node, Scope parentScope, SemanticAnalyzer semanticModel)
        {
            /*
            semanticModel.SetScope(node, parentScope);

            foreach (var child in node.GetChildNodes())
                Traverse(child, parentScope, semanticModel);
            */
        }
    }

    public class Symbol
    {
        public Symbol(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class Scope
    {
        Dictionary<string, List<Symbol>> _symbols = new();

        public Scope(Scope? parent)
        {
            Parent = parent;
        }

        public Scope? Parent { get; }

        public void AddSymbol(Symbol symbol)
        {
            if (!_symbols.TryGetValue(symbol.Name, out var list))
            {
                list = new List<Symbol>();
                _symbols.Add(symbol.Name, list);
            }

            list.Add(symbol);
        }

        public ImmutableArray<Symbol> GetAllMatchingSymbols(string name)
        {
            var currentScope = this;
            var result = ImmutableArray.CreateBuilder<Symbol>();
            while (currentScope != null)
            {
                if (currentScope._symbols.TryGetValue(name, out var list))
                    result.AddRange(list);
                currentScope = currentScope.Parent;
            }
            return result.ToImmutable();
        }
    }
}