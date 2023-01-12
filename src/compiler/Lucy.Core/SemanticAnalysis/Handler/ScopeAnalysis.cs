using System.Collections.Generic;
using System.Collections.Immutable;
using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors;
using Lucy.Core.SemanticAnalysis.Infrastructure;

namespace Lucy.Core.SemanticAnalysis.Handler
{
    public static class ScopeAnalysisHandler
    {
        [DbQuery]
        public static ScopeAnalysis GetScopeAnalysis(IDb db, string documentPath)
        {
            var tree = db.CreateScopeTree(documentPath);
            var errors = new List<Error>();

            Dictionary<INodeId<TokenNode>, ComparableReadOnlyList<ScopeEntry>> declarationOfIdentifiers = new();

            void Traverse(SubScope scope, ImmutableList<Dictionary<string, HashSet<ScopeEntry>>> stack)
            {
                foreach (var entry in tree.Entries)
                    if (entry is FunctionDeclarationScopeEntry fd)
                        AddToCurrentScope(fd.Name, fd);

                bool AddToCurrentScope(string name, ScopeEntry entry)
                {
                    if (stack[^1].TryGetValue(name, out var list))
                        return list.Add(entry);

                    list = new HashSet<ScopeEntry>();
                    stack[^1].Add(name, list);
                    return list.Add(entry);
                }

                foreach (var entry in tree.Entries)
                {
                    if (entry is FunctionDeclarationScopeEntry fd)
                    {
                        Traverse(fd.SubScope, stack.Add(new()));
                    }

                    if (entry is SubScope sc)
                    {
                        Traverse(sc, stack.Add(new()));
                    }

                    if (entry is VariableDeclarationScopeEntry vd)
                        if (!AddToCurrentScope(vd.Name, vd))
                            errors.Add(new ErrorWithNodeId(vd.NodeId, "A variable with this name was already declared in the same scope."));

                    if (entry is IdentifierUsed used)
                    {
                        var decs = new List<ScopeEntry>();
                        for (var i = stack.Count - 1; i >= 0; i--)
                        {
                            if (stack[i].TryGetValue(used.Name, out var list))
                                decs.AddRange(list);
                        }
                    }
                }
            }

            //Traverse(tree, ImmutableStack<Dictionary<string, HashSet<ScopeEntry>>>.Empty.Push(new Dictionary<string, HashSet<ScopeEntry>>()));
            return null!;
        }
    }

    public class ScopeAnalysis
    {

    }
}
