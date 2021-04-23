using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using Lucy.Core.Parser.Nodes.Token;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Lucy.Core.SemanticAnalysis
{
    internal class ScopeAssigner
    {
        internal static void Run(SyntaxNode node)
        {
            var rootScope = new Scope(null);
            Annotate(node, rootScope);
        }

        private static void Annotate(SyntaxNode node, Scope parentScope)
        {
            node.SetAnnotation(parentScope);

            foreach (var child in node.GetChildNodes())
                Annotate(child.Node, parentScope);
        }
    }

    public static class ScopeExtensionMethods
    {
        public static Scope GetScope(this SyntaxNode node)
        {
            return node.GetRequiredAnnotation<Scope>();
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
        Dictionary<string, List<Symbol>> _symbols = new Dictionary<string, List<Symbol>>();

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

        internal ImmutableArray<Symbol> GetAllMatchingSymbols(string name)
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