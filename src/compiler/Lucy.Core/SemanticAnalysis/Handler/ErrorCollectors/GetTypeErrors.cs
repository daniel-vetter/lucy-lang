using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure.Salsa;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors
{
    [QueryGroup]
    public class GetTypeErrors
    {
        private readonly Nodes _nodes;
        private readonly TypeResolver _typeResolver;

        public GetTypeErrors(Nodes nodes, TypeResolver typeResolver)
        {
            _nodes = nodes;
            _typeResolver = typeResolver;
        }
        
        public virtual ComparableReadOnlyList<Error> GetAllTypeErrors()
        {
            var result = new ComparableReadOnlyList<Error>.Builder();
            foreach (var document in _nodes.GetDocumentList())
                result.AddRange(GetAllTypeErrorsInDocument(document));
            return result.Build();
        }

        public virtual ComparableReadOnlyList<Error> GetAllTypeErrorsInDocument(string documentPath)
        {
            var result = new ComparableReadOnlyList<Error>.Builder();

            foreach (var nodeId in _nodes.GetNodeIdsByType<TypeReferenceSyntaxNode>(documentPath))
            {
                var info = _typeResolver.GetTypeInfo(nodeId);
                if (info != null)
                    continue;

                var node = _nodes.GetNodeById(nodeId);

                result.Add(new ErrorWithNodeId(node.TypeName.NodeId, $"The type '{node.TypeName.Text}' could not be found."));
            }

            return result.Build();
        }
    }
}
