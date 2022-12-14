using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using Lucy.Core.SemanticAnalysis.Inputs;

namespace Lucy.Core.SemanticAnalysis.Handler.ErrorCollectors
{
    public static class GetTypeErrors
    {
        [GenerateDbExtension] ///<see cref="GetAllTypeErrorsEx.GetAllTypeErrors" />
        public static ComparableReadOnlyList<Error> GetAllTypeErrors(IDb db)
        {
            var result = new ComparableReadOnlyList<Error>.Builder();
            foreach (var document in db.GetDocumentList())
                result.AddRange(db.GetAllTypeErrorsInDocument(document));
            return result.Build();
        }

        [GenerateDbExtension] ///<see cref="GetAllTypeErrorsInDocumentEx.GetAllTypeErrorsInDocument" />
        public static ComparableReadOnlyList<Error> GetAllTypeErrorsInDocument(IDb db, string documentPath)
        {
            var result = new ComparableReadOnlyList<Error>.Builder();

            foreach (var nodeId in db.GetNodeIdsByType<TypeReferenceSyntaxNode>(documentPath))
            {
                var info = db.GetTypeInfoFromTypeReferenceId(nodeId);
                if (info != null)
                    continue;

                var node = db.GetNodeById(nodeId);

                result.Add(new ErrorWithNodeId(node.TypeName.NodeId, $"The type '{node.TypeName.Text}' could not be found."));
            }

            return result.Build();
        }
    }
}
