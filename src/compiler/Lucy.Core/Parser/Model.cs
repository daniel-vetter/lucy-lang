using System;
using System.Collections.Generic;

namespace Lucy.Core.Model.Syntax
{
    public abstract class SyntaxNode
    {
        private Dictionary<Type, object> _annotations = new();

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
    }

    public abstract class TriviaNode : SyntaxNode
    {
    }
}