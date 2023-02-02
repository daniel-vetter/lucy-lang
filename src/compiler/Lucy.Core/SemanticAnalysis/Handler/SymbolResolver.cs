using System;
using System.Collections.Generic;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    [QueryGroup]
    public class SymbolResolver
    {
        private readonly ScopeTreeBuilder _scopeTreeBuilder;

        public SymbolResolver(ScopeTreeBuilder scopeTreeBuilder)
        {
            _scopeTreeBuilder = scopeTreeBuilder;
        }

        /// <summary>
        /// Returns a list of declarations based on a <see cref="TokenNode"/> id.
        /// Only declarations visible on the nodes locations are returned.
        /// </summary>
        /// <param name="nodeId">The <see cref="TokenNode"/> id which contains the symbol to look for</param>
        /// <returns>A list of matching <see cref="SymbolDeclaration"/></returns>
        /// <exception cref="Exception">if the provided token node id is not a valid symbol</exception>
        public virtual ComparableReadOnlyList<SymbolDeclaration> GetSymbolDeclarations(INodeId<TokenNode> nodeId)
        {
            var tree = GetSymbolMap(nodeId.DocumentPath);
            if (tree.TryGetValue(nodeId, out var list))
                return list;
            throw new Exception("No symbol resolution done for this node.");
        }

        /// <summary>
        /// Returns a list of all resolved symbol and there declarations of a document.
        /// Only declarations visible on the nodes locations are returned.
        /// </summary>
        /// <param name="documentPath">The document to analyze</param>
        /// <returns>a list of all resolved symbol and there declarations</returns>
        public virtual ComparableReadOnlyDictionary<INodeId<TokenNode>, ComparableReadOnlyList<SymbolDeclaration>> GetSymbolMap(string documentPath)
        {
            var scopeTree = _scopeTreeBuilder.GetScopeTree(documentPath);
            var stack = new List<Dictionary<string, List<SymbolDeclaration>>>();
            var result = new ComparableReadOnlyDictionary<INodeId<TokenNode>, ComparableReadOnlyList<SymbolDeclaration>>.Builder();
            stack.Add(new());

            void Traverse(Scope scope)
            {
                foreach (var entry in scope.Entries)
                {
                    if (entry is SymbolDeclaration sd)
                    {
                        if (!stack[^1].TryGetValue(sd.Name, out var list))
                        {
                            list = new List<SymbolDeclaration>();
                            stack[^1].Add(sd.Name, list);
                        }

                        stack[^1][sd.Name].Add(sd);
                    }

                    if (entry is Scope subScope)
                    {
                        stack.Add(new());
                        Traverse(subScope);
                        stack.RemoveAt(stack.Count - 1);
                    }

                    if (entry is SymbolUsage usage)
                    {
                        var allDeclarations = new ComparableReadOnlyList<SymbolDeclaration>.Builder();
                        for (int i = stack.Count - 1; i >= 0; i--)
                        {
                            if (!stack[i].TryGetValue(usage.Name, out var list))
                                continue;

                            allDeclarations.AddRange(list);
                        }

                        result.Add(usage.NameTokenNodeId, allDeclarations.Build());
                    }
                }
            }

            Traverse(scopeTree);

            return result.Build();
        }
    }
}