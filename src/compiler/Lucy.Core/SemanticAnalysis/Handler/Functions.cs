﻿using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public record FlatIdentifier(string Text, INodeId<TokenNode> NodeId);
public record FlatFunctionDeclaration(INodeId<FunctionDeclarationStatementSyntaxNode> NodeId, FlatIdentifier Name, ComparableReadOnlyList<FlatFunctionDeclarationParameter> Parameters, INodeId<TypeReferenceSyntaxNode> ReturnType);
public record FlatFunctionDeclarationParameter(INodeId<FunctionDeclarationParameterSyntaxNode> NodeId, FlatIdentifier Name, INodeId<TypeReferenceSyntaxNode>? TypeReference);

public record FlatFunctionCall(INodeId<FunctionCallExpressionSyntaxNode> NodeId, FlatIdentifier Name, ComparableReadOnlyList<INodeId<ExpressionSyntaxNode>> Arguments);
public record FlatFunctionParameterDeclaration(INodeId<FunctionDeclarationParameterSyntaxNode> NodeId, INodeId<TypeReferenceSyntaxNode> TypeReferenceNodeId, FlatIdentifier Name);
public record FunctionParameterInfo(string Name, INodeId<TokenNode> NameTokenNodeId, TypeReferenceInfo TypeReferenceInfo);
public record TypeReferenceInfo(INodeId<TypeReferenceSyntaxNode> TypeReferenceNodeId, TypeInfo? ResolvedType);

public static class FunctionsHandler
{
    [DbQuery] ///<see cref="GetFlatFunctionCallEx.GetFlatFunctionCall("/>
    public static FlatFunctionCall GetFlatFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> functionCallNodeId)
    {
        var node = db.GetNodeById(functionCallNodeId);

        return new FlatFunctionCall(
            functionCallNodeId, 
            new FlatIdentifier(node.FunctionName.Text, node.FunctionName.NodeId),
            node.ArgumentList.Select(x => x.Expression.NodeId).ToComparableReadOnlyList()
        );
    }

    [DbQuery] ///<see cref="GetFunctionsInDocumentEx.GetFunctionsInDocument"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetFunctionsInDocument(IDb db, string documentPath)
    {
        return db.GetNodeIdsByType<FunctionDeclarationStatementSyntaxNode>(documentPath)
            .Select(db.GetFlatFunctionDeclaration)
            .ToComparableReadOnlyList();
    }

    [DbQuery] ///<see cref="GetFlatFunctionDeclarationEx.GetFlatFunctionDeclaration"/>
    public static FlatFunctionDeclaration GetFlatFunctionDeclaration(IDb db, INodeId<FunctionDeclarationStatementSyntaxNode> functionDeclarationNodeId)
    {
        var node = db.GetNodeById(functionDeclarationNodeId);

        var parameters = node.ParameterList
            .Select(x => new FlatFunctionDeclarationParameter(
                x.NodeId, 
                new FlatIdentifier(
                    x.VariableDefinition.VariableName.Text,
                    x.VariableDefinition.VariableName.NodeId
                ),
                x.VariableDefinition.VariableType?.TypeReference.NodeId
            )).ToComparableReadOnlyList();

        return new FlatFunctionDeclaration(
            node.NodeId,
            new FlatIdentifier(
                node.FunctionName.Text, 
                node.FunctionName.NodeId
            ),
            parameters,
            node.ReturnType.TypeReference.NodeId
        );
    }

    [DbQuery] ///<see cref="GetDeclaredFunctionsInStatementListEx.GetDeclaredFunctionsInStatementList"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetDeclaredFunctionsInStatementList(IDb db, INodeId<StatementListSyntaxNode> statementListNodeId)
    {
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach (var statement in db.GetNodeById(statementListNodeId).Statements)
        {
            if (statement is FunctionDeclarationStatementSyntaxNode functionStatement)
                result.Add(db.GetFlatFunctionDeclaration(functionStatement.NodeId));
        }
        return result.Build();
    }

    [DbQuery] ///<see cref="GetReachableFunctionsInStatementListEx.GetReachableFunctionsInStatementList" />
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetReachableFunctionsInStatementList(IDb db, INodeId<StatementListSyntaxNode> statementListNodeId)
    {
        var currentNode = statementListNodeId;
        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        while (true)
        {
            result.AddRange(db.GetDeclaredFunctionsInStatementList(currentNode));

            currentNode = db.GetParentNodeIdOfType<StatementListSyntaxNode>(currentNode);
            if (currentNode == null)
                break;
        }

        foreach (var import in db.GetImports(statementListNodeId.DocumentPath).Valid)
        {
            result.AddRange(db.GetFunctionsInDocument(import.Path));
        }
        return result.Build();
    }

    [DbQuery] ///<see cref="GetReachableFunctionsInScopeEx.GetReachableFunctionsInScope" />
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetReachableFunctionsInScope(IDb db, INodeId<SyntaxTreeNode> scopeTarget)
    {
        var sl = scopeTarget as INodeId<StatementListSyntaxNode>
                 ?? db.GetParentNodeIdOfType<StatementListSyntaxNode>(scopeTarget);

        if (sl is null)
            throw new Exception("Invalid scopeTarget");

        return db.GetReachableFunctionsInStatementList(sl);
    }

    [DbQuery] ///<see cref="GetFunctionsWithNameInScopeEx.GetFunctionsWithNameInScope" />
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetFunctionsWithNameInScope(IDb db, INodeId<SyntaxTreeNode> scopeTarget, string name)
    {
        return db.GetReachableFunctionsInScope(scopeTarget)
            .Where(x => x.Name.Text == name)
            .ToComparableReadOnlyList();
    }


    [DbQuery] ///<see cref="GetFunctionCandidatesFromFunctionCallEx.GetFunctionCandidatesFromFunctionCall"/>
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetFunctionCandidatesFromFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> nodeId)
    {
        var functionCallInfo = db.GetFlatFunctionCall(nodeId);
        return db.GetFunctionsWithNameInScope(functionCallInfo.NodeId, functionCallInfo.Name.Text);
    }

    [DbQuery] ///<see cref="GetAllMatchingFunctionsFromFunctionCallEx.GetAllMatchingFunctionsFromFunctionCall" />
    public static ComparableReadOnlyList<FlatFunctionDeclaration> GetAllMatchingFunctionsFromFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> functionCallExpressionNodeId)
    {
        var functionCandidates = db.GetFunctionCandidatesFromFunctionCall(functionCallExpressionNodeId);

        var argumentTypes = db.GetFunctionArgumentTypes(functionCallExpressionNodeId);
        if (argumentTypes.Any(x => x is null))
            return new ComparableReadOnlyList<FlatFunctionDeclaration>();

        var result = new ComparableReadOnlyList<FlatFunctionDeclaration>.Builder();
        foreach (var flatFunctionDeclaration in functionCandidates)
        {
            var functionParameterTypes = db.GetFunctionParameterTypes(flatFunctionDeclaration.NodeId);
            if (functionParameterTypes.Any(x => x is null))
                continue;

            if (functionParameterTypes.Count  != argumentTypes.Count)
                continue;

            var match = true;
            for (var i = 0; i < argumentTypes.Count; i++)
            {
                if (argumentTypes[i] != functionParameterTypes[i])
                {
                    match = false;
                    break;
                }
            }

            if (!match)
                continue;
            
            result.Add(flatFunctionDeclaration);
        }

        return result.Build();
    }

    [DbQuery] ///<see cref="GetBestMatchingFunctionsFromFunctionCallEx.GetBestMatchingFunctionsFromFunctionCall" />
    public static FlatFunctionDeclaration? GetBestMatchingFunctionsFromFunctionCall(IDb db, INodeId<FunctionCallExpressionSyntaxNode> functionCallExpressionNodeId)
    {
        var all = db.GetAllMatchingFunctionsFromFunctionCall(functionCallExpressionNodeId);
        return all.Count != 1 ? null : all[0];
    }

    [DbQuery] ///<see cref="GetFunctionParameterTypesEx.GetFunctionParameterTypes" />
    public static ComparableReadOnlyList<TypeInfo?> GetFunctionParameterTypes(IDb db, INodeId<FunctionDeclarationStatementSyntaxNode> nodeId)
    {
        var flat = db.GetFlatFunctionDeclaration(nodeId);
        var parameterTypes = new ComparableReadOnlyList<TypeInfo?>.Builder();
        foreach (var parameter in flat.Parameters)
        {
            parameterTypes.Add(db.GetTypeInfoFromTypeReferenceId(parameter.TypeReference));
        }
        return parameterTypes.Build();
    }

    [DbQuery] ///<see cref="GetFunctionArgumentTypesEx.GetFunctionArgumentTypes" />
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

    [DbQuery] ///<see cref="GetExpressionTypeEx.GetExpressionType" />
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
                var functionDeclaration = db.GetBestMatchingFunctionsFromFunctionCall(fc.NodeId);
                if (functionDeclaration == null)
                    return null;

                return db.GetTypeInfoFromTypeReferenceId(functionDeclaration.ReturnType);
            case VariableReferenceExpressionSyntaxNode vr:
                return db.GetTypeInfoFromVariableReference(vr.NodeId);
            default:
                throw new ArgumentOutOfRangeException(nameof(node), "Can not resolve type of expression node " + node.GetType().Name + " because it is not a known type.");
        }
    }
}