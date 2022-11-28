﻿using Lucy.Core.Model;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.SemanticAnalysis.Infrastructure;
using System.Linq;

namespace Lucy.Core.SemanticAnalysis.Handler;

public record FunctionInfo(string Name, NodeId DeclarationNodeId, NodeId NameTokenNodeId, ComparableReadOnlyList<FunctionParameterInfo> Parameters);
public record FunctionParameterInfo(string Name, NodeId NameTokenNodeId, NodeId TypeReferenceNodeId);
public record FunctionCallInfo(string Name, NodeId DeclarationNodeId, NodeId NameTokenNodeId);

public static class FunctionsHandler
{
    [GenerateDbExtension] ///<see cref="GetFunctionInfoEx.GetFunctionInfo"/>
    public static FunctionCallInfo GetFunctionCallInfo(IDb db, NodeId functionCallNodeId)
    {
        var node = (FunctionCallExpressionSyntaxNode)db.GetNodeById(functionCallNodeId);
        return new FunctionCallInfo(node.FunctionName.Token.Text, functionCallNodeId, node.FunctionName.Token.NodeId);
    }

    [GenerateDbExtension] ///<see cref="GetFunctionsInDocumentEx.GetFunctionsInDocument"/>
    public static ComparableReadOnlyList<FunctionInfo> GetFunctionsInDocument(IDb db, string documentPath)
    {
        return db.GetNodeIdByType<FunctionDeclarationStatementSyntaxNode>(documentPath)
            .Select(db.GetFunctionInfo)
            .ToComparableReadOnlyList();
    }

    [GenerateDbExtension] ///<see cref="GetFunctionInfoEx.GetFunctionInfo"/>
    public static FunctionInfo GetFunctionInfo(IDb db, NodeId functionDeclarationNodeId)
    {
        var node = (FunctionDeclarationStatementSyntaxNode)db.GetNodeById(functionDeclarationNodeId);

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
    public static ComparableReadOnlyList<FunctionInfo> GetFunctionsInStatementList(IDb db, NodeId statementListNodeId)
    {
        var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
        foreach (var statement in ((StatementListSyntaxNode)db.GetNodeById(statementListNodeId)).Statements)
        {
            if (statement is FunctionDeclarationStatementSyntaxNode)
                result.Add(db.GetFunctionInfo(statement.NodeId));
        }
        return result.Build();
    }

    [GenerateDbExtension] ///<see cref="GetFunctionCandidatesFromFunctionCallEx.GetFunctionCandidatesFromFunctionCall"/>
    public static ComparableReadOnlyList<FunctionInfo> GetFunctionCandidatesFromFunctionCall(IDb db, NodeId functionCallExpressionNodeId)
    {
        var result = new ComparableReadOnlyList<FunctionInfo>.Builder();
        var functionCallInfo = db.GetFunctionCallInfo(functionCallExpressionNodeId);
        var currentNode = functionCallExpressionNodeId;

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