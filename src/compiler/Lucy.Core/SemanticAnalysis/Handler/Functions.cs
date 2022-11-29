using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public record FunctionInfo(string Name, INodeId<FunctionDeclarationStatementSyntaxNode> DeclarationNodeId, INodeId<TokenNode> NameTokenNodeId, ComparableReadOnlyList<FunctionParameterInfo> Parameters);
public record FunctionParameterInfo(string Name, INodeId<TokenNode> NameTokenNodeId, INodeId<TypeReferenceSyntaxNode> TypeReferenceNodeId);
public record FunctionCallInfo(string Name, INodeId<FunctionCallExpressionSyntaxNode> DeclarationNodeId, INodeId<TokenNode> NameTokenNodeId);

public static class FunctionsHandler
{
    [GenerateDbExtension] ///<see cref="GetFunctionInfoEx.GetFunctionInfo"/>
    public static FunctionCallInfo GetFunctionCallInfo(IDb db, INodeId<FunctionCallExpressionSyntaxNode> functionCallNodeId)
    {
        var node = db.GetNodeById(functionCallNodeId);
        return new FunctionCallInfo(node.FunctionName.Token.Text, functionCallNodeId, node.FunctionName.Token.NodeId);
    }

    [GenerateDbExtension] ///<see cref="GetFunctionsInDocumentEx.GetFunctionsInDocument"/>
    public static ComparableReadOnlyList<FunctionInfo> GetFunctionsInDocument(IDb db, string documentPath)
    {
        return db.GetNodeIdsByType<FunctionDeclarationStatementSyntaxNode>(documentPath)
            .Select(db.GetFunctionInfo)
            .ToComparableReadOnlyList();
    }

    [GenerateDbExtension] ///<see cref="GetFunctionInfoEx.GetFunctionInfo"/>
    public static FunctionInfo GetFunctionInfo(IDb db, INodeId<FunctionDeclarationStatementSyntaxNode> functionDeclarationNodeId)
    {
        var node = db.GetNodeById(functionDeclarationNodeId);

        var parameters = node.ParameterList.Select(x => new FunctionParameterInfo(
            x.VariableDeclaration.VariableName.Token.Text,
            x.VariableDeclaration.VariableName.Token.NodeId,
            x.VariableDeclaration.TypeReference.NodeId))
            .ToComparableReadOnlyList();

        return new FunctionInfo(
            node.FunctionName.Token.Text,
            functionDeclarationNodeId,
            node.FunctionName.Token.NodeId,
            parameters
        );
    }

    [GenerateDbExtension] ///<see cref="GetFunctionsInStatementListEx.GetFunctionsInStatementList"/>
    public static ComparableReadOnlyList<FunctionInfo> GetFunctionsInStatementList(IDb db, INodeId<StatementListSyntaxNode> statementListNodeId)
    {
        var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
        foreach (var statement in db.GetNodeById(statementListNodeId).Statements)
        {
            if (statement is FunctionDeclarationStatementSyntaxNode functionStatement)
                result.Add(db.GetFunctionInfo(functionStatement.NodeId));
        }
        return result.Build();
    }

    [GenerateDbExtension] ///<see cref="GetFunctionCandidatesFromFunctionCallEx.GetFunctionCandidatesFromFunctionCall"/>
    public static ComparableReadOnlyList<FunctionInfo> GetFunctionCandidatesFromFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> functionCallExpressionNodeId)
    {
        var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
        var functionCallInfo = db.GetFunctionCallInfo(functionCallExpressionNodeId);
        INodeId<SyntaxTreeNode> currentNode = functionCallExpressionNodeId;

        while (true)
        {
            var parent = db.GetNearestParentNodeOfType<StatementListSyntaxNode>(currentNode);
            if (parent == null)
                break;

            var functions = db.GetFunctionsInStatementList(parent);
            result.AddRange(functions.Where(x => x.Name == functionCallInfo.Name));

            currentNode = parent;
        }

        foreach (var import in db.GetImports(functionCallExpressionNodeId.DocumentPath).Valid)
        {
            result.AddRange(db.GetFunctionsInDocument(import.Path).Where(x => x.Name == functionCallInfo.Name));
        }

        return result.Build();

    }
}