﻿using Lucy.Core.Model;
using Lucy.Core.SemanticAnalysis;
using Lucy.Core.SemanticAnalysis.Handler;

namespace Lucy.Interpreter
{
    public static class CodeInterpreter
    {
        public static Value Run(SemanticAnalyzer semanticAnalyzer)
        {
            var entryPoint = semanticAnalyzer.Get<EntryPointFinder>().GetEntryPoints();
            if (entryPoint.Count != 1)
                throw new Exception("No entry point found.");

            var entryPointNode = semanticAnalyzer.Get<Nodes>().GetNodeById(entryPoint[0].NodeId);

            return Run(entryPointNode, new InterpreterContext(semanticAnalyzer));
        }

        private static Value Run(SyntaxTreeNode node, InterpreterContext ctx)
        {
            return node switch
            {
                FunctionDeclarationStatementSyntaxNode fdsn => Handle(fdsn, ctx),
                FunctionCallExpressionSyntaxNode fcesn => Handle(fcesn, ctx),
                StringConstantExpressionSyntaxNode sc => new StringValue(sc.Value),
                NumberConstantExpressionSyntaxNode nc => new NumberValue(nc.Value),
                AdditionExpressionSyntaxNode a => Handle(a, ctx),
                MemberAccessExpressionSyntaxNode ma => Handle(ma, ctx),
                AndExpressionSyntaxNode a => Handle(a, ctx),
                OrExpressionSyntaxNode o => Handle(o, ctx),
                IfExpressionSyntaxNode @if => Handle(@if, ctx),
                StatementListSyntaxNode sln => Handle(sln, ctx),
                ExpressionStatementSyntaxNode esn => Handle(esn, ctx),
                _ => throw new NotSupportedException("Unsupported node type: " + node.GetType().Name)
            };
        }

        private static Value Handle(FunctionDeclarationStatementSyntaxNode functionDeclarationStatementSyntaxNode, InterpreterContext ctx)
        {
            return new VoidValue();
        }

        private static Value Handle(FunctionCallExpressionSyntaxNode functionCallExpressionSyntaxNode, InterpreterContext ctx)
        {
            var target = ctx.SemanticAnalyzer.Get<Functions>().GetBestFunctionCallTarget(functionCallExpressionSyntaxNode.NodeId);
            if (target is not INodeId<FunctionDeclarationStatementSyntaxNode> funcDecId)
                throw new Exception("Invalid function call target");

            var funDec = ctx.SemanticAnalyzer.Get<Nodes>().GetNodeById(funcDecId);
            if (funDec.ExternFunctionName == null || funDec.ExternLibraryName == null)
                throw new NotImplementedException("Only external functions are currently supported.");

            var arguments = new List<object>();
            foreach(var argument in functionCallExpressionSyntaxNode.ArgumentList)
            {
                var value = Run(argument.Expression, ctx);
                if (value is StringValue sv) arguments.Add(sv.Value);
                else if (value is NumberValue nv) arguments.Add((int)nv.Value);
                else throw new Exception("Could not unwrap value of type " + value.GetType().Name);
            }

            NativeLib.Call(funDec.ExternLibraryName.Str.Text, funDec.ExternFunctionName.Str.Text, null, arguments.ToArray());

            return new VoidValue();
        }

        private static Value Handle(StatementListSyntaxNode statementListNode, InterpreterContext ctx)
        {
            foreach(var statement in statementListNode.Statements)
            {
                Run(statement, ctx);
            }
            return new VoidValue();
        }

        private static Value Handle(ExpressionStatementSyntaxNode expressionStatementNode, InterpreterContext ctx)
        {
            Run(expressionStatementNode.Expression, ctx);
            return new VoidValue();
        }

        private static Value Handle(AndExpressionSyntaxNode andNode, InterpreterContext ctx)
        {
            var left = Run(andNode.Left, ctx);
            var right = Run(andNode.Right, ctx);

            if (left is not BooleanValue bLeft || right is not BooleanValue bRight)
                throw new Exception("Can not compare type " + left.GetType() + " with " + right.GetType().Name);

            return new BooleanValue(bLeft == bRight);
        }

        private static Value Handle(OrExpressionSyntaxNode orNode, InterpreterContext ctx)
        {
            var left = Run(orNode.Left, ctx);
            var right = Run(orNode.Right, ctx);

            if (left is BooleanValue b && b.Value == true)
                return left;

            if (left is not BooleanValue bLeft || right is not BooleanValue bRight)
                throw new Exception("Can not compare type " + left.GetType() + " with " + right.GetType().Name);

            return new BooleanValue(bLeft.Value || bRight.Value);
        }

        private static Value Handle(IfExpressionSyntaxNode ifExpressionNode, InterpreterContext ctx)
        {
            var result = Run(ifExpressionNode.Condition, ctx);
            if (result is not BooleanValue booleanValue)
                throw new Exception("if expression condition must result in a boolean value");

            if (booleanValue.Value)
                return Run(ifExpressionNode.ThenExpression, ctx);

            return Run(ifExpressionNode.ElseExpression, ctx);
        }

        private static Value Handle(MemberAccessExpressionSyntaxNode memberAccess, InterpreterContext ctx)
        {
            var target = Run(memberAccess.Target, ctx);
            return new StringValue(""); //TODO
        }

        private static Value Handle(AdditionExpressionSyntaxNode stringConcatination, InterpreterContext ctx)
        {
            var left = Run(stringConcatination.Left, ctx);
            var right = Run(stringConcatination.Right, ctx);

            if (left is not NumberValue leftNumber || right is not NumberValue rightNumber)
                throw new Exception("Can not add type " + left.GetType() + " to " + right.GetType().Name);

            return new NumberValue(leftNumber.Value + rightNumber.Value);
        }
    }

    public class InterpreterContext
    {
        public InterpreterContext(SemanticAnalyzer semanticAnalyzer)
        {
            SemanticAnalyzer = semanticAnalyzer;
        }

        public SemanticAnalyzer SemanticAnalyzer { get; }
        public Dictionary<string, Value> Variables { get; } = new();
    }

    public abstract record Value;
    public record VoidValue : Value { }
    public record NullValue : Value;
    public record BooleanValue(bool Value) : Value;
    public record StringValue(string Value) : Value;
    public record NumberValue(double Value) : Value;
}
