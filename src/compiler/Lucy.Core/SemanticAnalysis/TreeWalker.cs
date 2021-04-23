using Lucy.Core.Helper;
using Lucy.Core.Model.Syntax;
using System.Collections.Generic;

namespace Lucy.Core.SemanticAnalysis
{
    public class TreeWalker
    {
        private List<IVisitor> _visitors = new();

        public void Walk(SyntaxNode node)
        {
            var ctx = new TreeWalkerContext(node, null, _visitors.ToArray());

            foreach(var v in _visitors)
            {
                v.Visit(node, ctx);
            }
        }

        public void AddVistor(IVisitor visitor) => _visitors.Add(visitor);
    }

    public interface IVisitor
    {
        public void Visit(SyntaxNode node, TreeWalkerContext ctx);
    }

    public class TreeWalkerContext
    {
        private readonly IVisitor[] _visitors;
        private SyntaxNode _node;
        private readonly TreeWalkerContext? _parentContext;

        public TreeWalkerContext(SyntaxNode node, TreeWalkerContext? parentContext, IVisitor[] visitors)
        {
            _node = node;
            _parentContext = parentContext;
            _visitors = visitors;
        }

       // public IReadOnlyCollection<SyntaxNode> Stack => _stack;
       // public SyntaxNode Current => _current;
        public SyntaxNode? Parent => _parentContext?._node;

        public void VisitAllChildNodes()
        {
            
                foreach (var c in _node.GetChildNodes())
                foreach (var v in _visitors)
                {
                    v.Visit(c.Node, new TreeWalkerContext(c.Node, this, _visitors));
                }
                    
        }
    }
}
