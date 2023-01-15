﻿using System;
using System.Collections.Generic;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class SymbolMap
    {
        [DbQuery]
        public static ComparableReadOnlyList<SymbolDeclaration> GetSymbolDeclarations(IDb db, INodeId<TokenNode> nodeId)
        {
            var tree = db.GetSymbolMap(nodeId.DocumentPath);
            if (tree.TryGetValue(nodeId, out var list))
                return list;
            throw new Exception("No symbol resolution done for this node.");
        }

        [DbQuery]
        public static ComparableReadOnlyDictionary<INodeId<TokenNode>, ComparableReadOnlyList<SymbolDeclaration>> GetSymbolMap(IDb db, string documentPath)
        {
            var scopeTree = db.GetScopeTree(documentPath);
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
                        for (int i=stack.Count-1;i>= 0;i--)
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
