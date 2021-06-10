using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lucy.Core.Model.Syntax
{
    public static class SyntaxTreeNodeEx
    {
        public static T SetAnnotation<T>(this T node, object annotation) where T : SyntaxTreeNode
        {
            node.Annotations[annotation.GetType()] = annotation;
            return node;
        }
    }

    public abstract class SyntaxTreeNode
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public Dictionary<Type, object> Annotations { get; } = new();

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private DebugKeyValuePair[] AnnotationsDebugView => Annotations.Select(x => new DebugKeyValuePair(x.Key.Name, x.Value)).ToArray();

        public T? GetAnnotation<T>() where T : class
        {
            if (!Annotations.TryGetValue(typeof(T), out var annotation))
                return null;
            return (T)annotation;
        }

        public T GetRequiredAnnotation<T>() where T : class
        {
            if (!Annotations.TryGetValue(typeof(T), out var annotation))
                throw new Exception($"Could not find required annotation {typeof(T).Name} on node {GetType().Name}.");
            return (T)annotation;
        }

        [DebuggerDisplay("{_value}", Name = "{_name,nq}", TargetTypeName = "_value")]
        private class DebugKeyValuePair
        {
            private readonly string _name;
            private readonly object _value;

            public DebugKeyValuePair(string name, object value)
            {
                _name = name;
                _value = value;
            }
        }

        public SyntaxTreeNodeSource Source { get; set; } = new SourceCode();
    }

    public abstract class SyntaxTreeNodeSource
    {
    }

    public class Syntetic : SyntaxTreeNodeSource
    {
        public Syntetic(string? errorMessage, Position? position)
        {
            ErrorMessage = errorMessage;
            Position = position;
        }

        public string? ErrorMessage { get; }
        public Position? Position { get; set; }
    }

    public class SourceCode : SyntaxTreeNodeSource
    {
        public Range? Range { get; set; }
    }

    public class Generated : SyntaxTreeNodeSource
    {
    }

    public class TokenNode : SyntaxTreeNode
    {
        public TokenNode(string text = "")
        {
            Text = text;
        }

        public static TokenNode Missing(string? errorMessage = null)
        {
            return new TokenNode("") { Source = new Syntetic(errorMessage, null) };
        }

        public string Text { get; }
    }

    public record Range(Position Start, Position End)
    {
        public override string ToString() => $"{Start.Index} - {End.Index}";

        public bool Contains(int index) => index >= Start.Index && index < End.Index;
        public bool Contains(int line, int column)
        {
            if (line < Start.Line || line > End.Line)
                return false;

            var isAfterStart = (line == Start.Line && column >= Start.Column) || (line > Start.Line);
            var isBeforeEnd = (line == End.Line && column < End.Column) || (line < End.Line);
            return isAfterStart && isBeforeEnd;
        }
    }

    public record Position(int Index, int Line, int Column)
    {
        public Position Append(string str)
        {
            var character = Index + str.Length;
            var line = Line;
            var column = Column;

            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '\n')
                {
                    line++;
                    column = 0;
                }
                else
                {
                    column++;
                }
            }

            return new Position(character, line, column);
        }
    }
}