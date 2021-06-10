using Lucy.Core.Parsing.Nodes.Expressions.Nested;
using Lucy.Core.Parsing.Nodes.Expressions.Unary;
using Lucy.Core.Parsing.Nodes.Statements;
using Lucy.Core.Parsing;
using System;
using System.Collections.Generic;

namespace Lucy.Core.Interpreter
{
    public static class TreeInterpreter
    {
        public static Value Run(SyntaxTreeNode node, InterpreterContext ctx)
        {
            return node switch
            {
                StringConstantExpressionSyntaxNode sc => new StringValue(sc.Value),
                NumberConstantExpressionSyntaxNode nc => new NumberValue(nc.Value),
                AdditionExpressionSyntaxNode a => Handle(a, ctx),
                //IdentifierNode i => Handle(i, ctx),
                MemberAccessExpressionSyntaxNode ma => Handle(ma, ctx),
                AndExpressionSyntaxNode a => Handle(a, ctx),
                OrExpressionSyntaxNode o => Handle(o, ctx),
                IfExpressionSyntaxNode @if => Handle(@if, ctx),
                StatementListSyntaxNode sln => Handle(sln, ctx),
                ExpressionStatementSyntaxNode esn => Handle(esn, ctx),
                _ => throw new NotSupportedException("Unsupported node type: " + node.GetType().Name)
            };
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
        public Dictionary<string, Value> Variables = new Dictionary<string, Value>();
    }

    public abstract record Value;
    public record VoidValue : Value { }
    public record NullValue : Value;
    public record BooleanValue(bool Value) : Value;
    public record StringValue(string Value) : Value;
    public record NumberValue(double Value) : Value;

}
