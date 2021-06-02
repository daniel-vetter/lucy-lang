using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Lucy.Core.Model.Syntax
{
    public abstract class SyntaxTreeNode
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Dictionary<Type, object> _annotations = new();

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        private DebugKeyValuePair[] Annotations => _annotations.Select(x => new DebugKeyValuePair(x.Key.Name, x.Value)).ToArray();

        public void SetAnnotation(object annotation) => _annotations[annotation.GetType()] = annotation;

        public T? GetAnnotation<T>() where T : class
        {
            if (!_annotations.TryGetValue(typeof(T), out var annotation))
                return null;
            return (T)annotation;
        }

        public T GetRequiredAnnotation<T>() where T : class
        {
            if (!_annotations.TryGetValue(typeof(T), out var annotation))
                throw new Exception($"Could not find required annotation {typeof(T).Name} on node {GetType().Name}.");
            return (T)annotation;
        }

        public void ClearAnnotations() => _annotations.Clear();

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
    }

    public class TokenNode : SyntaxTreeNode
    {
        public TokenNode(string text)
        {
            Text = text;
        }

        public string Text { get; }
    }

    
}