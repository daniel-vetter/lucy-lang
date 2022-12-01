using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public record FlatIdentifier(string Text, INodeId<TokenNode> NodeId);
public record FlatFunctionDeclaration(INodeId<FunctionDeclarationStatementSyntaxNode> NodeId, FlatIdentifier Name, ComparableReadOnlyList<FlatFunctionDeclarationParameter> Parameters, INodeId<TypeReferenceSyntaxNode> ReturnType);
public record FlatFunctionDeclarationParameter(INodeId<FunctionDeclarationParameterSyntaxNode> NodeId, FlatIdentifier Name, FlatVariableDeclaration VariableDeclaration);

public record FlatFunctionCall(INodeId<FunctionCallExpressionSyntaxNode> NodeId, FlatIdentifier Name, ComparableReadOnlyList<INodeId<ExpressionSyntaxNode>> Arguments);
public record FlatVariableDeclaration(INodeId<VariableNameWithTypeDeclarationSyntaxNode> NodeId, INodeId<TypeReferenceSyntaxNode> TypeReferenceNodeId, FlatIdentifier Name);
public record FunctionParameterInfo(string Name, INodeId<TokenNode> NameTokenNodeId, TypeReferenceInfo TypeReferenceInfo);
public record TypeReferenceInfo(INodeId<TypeReferenceSyntaxNode> TypeReferenceNodeId, TypeInfo? ResolvedType);

public static class FunctionsHandler
{
    [GenerateDbExtension] ///<see cref="GetFlatFunctionCallEx.GetFlatFunctionCall("/>
    public static FlatFunctionCall GetFlatFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> functionCallNodeId)
    {
        var node = db.GetNodeById(functionCallNodeId);

        return new FlatFunctionCall(
            functionCallNodeId, 
            new FlatIdentifier(node.FunctionName.Token.Text, node.FunctionName.Token.NodeId),
            node.ArgumentList.Select(x => x.Expression.NodeId).ToComparableReadOnlyList()
        );
    }

    [GenerateDbExtension] ///<see cref="GetFunctionsInDocumentEx.GetFunctionsInDocument"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetFunctionsInDocument(IDb db, string documentPath)
    {
        return db.GetNodeIdsByType<FunctionDeclarationStatementSyntaxNode>(documentPath)
            .Select(db.GetFlatFunctionDeclaration)
            .ToComparableReadOnlyList();
    }

    [GenerateDbExtension] ///<see cref="GetFlatFunctionDeclarationEx.GetFlatFunctionDeclaration"/>
    public static FlatFunctionDeclaration GetFlatFunctionDeclaration(IDb db, INodeId<FunctionDeclarationStatementSyntaxNode> functionDeclarationNodeId)
    {
        var node = db.GetNodeById(functionDeclarationNodeId);

        var parameters = node.ParameterList
            .Select(x => new FlatFunctionDeclarationParameter(
                x.NodeId, 
                new FlatIdentifier(
                    x.VariableDeclaration.VariableName.Token.Text,
                    x.VariableDeclaration.VariableName.Token.NodeId
                ),
                new FlatVariableDeclaration(
                    x.VariableDeclaration.NodeId,
                    x.VariableDeclaration.TypeReference.NodeId,
                    new FlatIdentifier(
                        x.VariableDeclaration.VariableName.Token.Text, 
                        x.VariableDeclaration.VariableName.Token.NodeId
                    )
                )
            )).ToComparableReadOnlyList();

        return new FlatFunctionDeclaration(
            node.NodeId,
            new FlatIdentifier(
                node.FunctionName.Token.Text, 
                node.FunctionName.Token.NodeId
            ),
            parameters,
            node.ReturnType.NodeId
        );
    }

    [GenerateDbExtension] ///<see cref="GetFunctionsInStatementListEx.GetFunctionsInStatementList"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetFunctionsInStatementList(IDb db, INodeId<StatementListSyntaxNode> statementListNodeId)
    {
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach (var statement in db.GetNodeById(statementListNodeId).Statements)
        {
            if (statement is FunctionDeclarationStatementSyntaxNode functionStatement)
                result.Add(db.GetFlatFunctionDeclaration(functionStatement.NodeId));
        }
        return result.Build();
    }

    [GenerateDbExtension] ///<see cref="GetFunctionCandidatesFromFunctionCallEx.GetFunctionCandidatesFromFunctionCall"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetFunctionCandidatesFromFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> nodeId)
    {
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        var functionCallInfo = db.GetFlatFunctionCall(nodeId);
        INodeId<SyntaxTreeNode> currentNode = nodeId;

        while (true)
        {
            var parent = db.GetNearestParentNodeOfType<StatementListSyntaxNode>(currentNode);
            if (parent == null)
                break;

            var functions = db.GetFunctionsInStatementList(parent);
            result.AddRange(functions.Where(x => x.Name.Text == functionCallInfo.Name.Text));

            currentNode = parent;
        }

        foreach (var import in db.GetImports(nodeId.DocumentPath).Valid)
        {
            result.AddRange(db.GetFunctionsInDocument(import.Path).Where(x => x.Name.Text == functionCallInfo.Name.Text));
        }

        return result.Build();
    }

    [GenerateDbExtension] ///<see cref="GetMatchingFunctionFromFunctionCallEx.GetMatchingFunctionFromFunctionCall" />
    public static FlatFunctionDeclaration? GetMatchingFunctionFromFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> functionCallExpressionNodeId)
    {
        var functionCandidates = db.GetFunctionCandidatesFromFunctionCall(functionCallExpressionNodeId);

        var argumentTypes = db.GetFunctionArgumentTypes(functionCallExpressionNodeId);
        if (argumentTypes.Any(x => x is null))
            return null;

        foreach (var flatFunctionDeclaration in functionCandidates)
        {
            var functionParameterTypes = db.GetFunctionParameterTypes(flatFunctionDeclaration.NodeId);
            if (functionParameterTypes.Any(x => x is null))
                continue;

            return flatFunctionDeclaration;
        }

        return null;
    }

    [GenerateDbExtension] ///<see cref="GetFunctionParameterTypesEx.GetFunctionParameterTypes" />
    public static ComparableReadOnlyList<TypeInfo?> GetFunctionParameterTypes(IDb db, INodeId<FunctionDeclarationStatementSyntaxNode> nodeId)
    {
        var flat = db.GetFlatFunctionDeclaration(nodeId);
        var parameterTypes = new ComparableReadOnlyList<TypeInfo?>.Builder();
        foreach (var parameter in flat.Parameters)
        {
            var typeInfo = db.GetTypeInfoFromTypeReferenceId(parameter.VariableDeclaration.TypeReferenceNodeId);
            parameterTypes.Add(typeInfo);
        }
        return parameterTypes.Build();
    }

    [GenerateDbExtension] ///<see cref="GetFunctionArgumentTypesEx.GetFunctionArgumentTypes" />
    public static ComparableReadOnlyList<TypeInfo?> GetFunctionArgumentTypes(IDb db, INodeId<FunctionCallExpressionSyntaxNode> nodeId)
    {
        var flatFunctionCall = db.GetFlatFunctionCall(nodeId);
        var list = new ComparableReadOnlyList<TypeInfo?>.Builder();

        foreach (var argument in flatFunctionCall.Arguments)
        {
            list.Add(db.GetExpressionType(argument));
        }

        return list.Build();
    }

    [GenerateDbExtension] ///<see cref="GetExpressionTypeEx.GetExpressionType" />
    public static TypeInfo? GetExpressionType(IDb db, INodeId<ExpressionSyntaxNode> nodeId)
    {
        var node = db.GetNodeById(nodeId);

        switch (node)
        {
            case StringConstantExpressionSyntaxNode:
                return KnownTypes.String;
            case NumberConstantExpressionSyntaxNode:
                return KnownTypes.Int32;
            case FunctionCallExpressionSyntaxNode fc:
                var functionDeclaration = db.GetMatchingFunctionFromFunctionCall(fc.NodeId);
                if (functionDeclaration == null)
                    return null;

                return db.GetTypeInfoFromTypeReferenceId(functionDeclaration.ReturnType);
                
            default:
                throw new ArgumentOutOfRangeException(nameof(node));
        }
    }
}