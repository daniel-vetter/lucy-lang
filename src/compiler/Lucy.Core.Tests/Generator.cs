using Lucy.Core.Parsing;
using Lucy.Core.Parsing.Nodes;
using Lucy.Core.Parsing.Nodes.Expressions;
using System.Collections;
using System.Collections.Immutable;

namespace Lucy.Core.Tests
{
    public class Generator
    {
        public static object Create(Type type, int seed)
        {
            var r = new Random(seed);
            return Create(type, r, ImmutableList<Type>.Empty);
        }

        private static object Create(Type type, Random r, ImmutableList<Type> stack)
        {
            if (stack.Count > 50)
                throw new Exception("Depth limit reached: " + string.Join("\n", stack));

            if (type.IsAbstract)
            {
                var pt = GetPossibleTypes(type);
                if (pt.Length == 0)
                    throw new Exception("No implementations for " + type.Name + " found.");
                type = pt[r.Next(pt.Length)];
            }

            if (type == typeof(string))
            {
                var sb = new char[r.Next(10, 20)];
                for (int i = 0; i < sb.Length; i++)
                    sb[i] = (char)('a' + r.Next(26));
                return new string(sb);
            }

            if (type == typeof(int))
                return r.Next();

            if (type == typeof(double))
                return r.NextDouble();

            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(ComparableReadOnlyList<>)))
            {
                var list = (IList)(Activator.CreateInstance(typeof(List<>).MakeGenericType(type.GetGenericArguments()[0])) ?? throw new Exception("Could not create list"));
                for (int i = 0; i < r.Next(0, 3); i++)
                {
                    list.Add(Create(type.GetGenericArguments()[0], r, stack.Add(type)));
                }

                var genericType = typeof(ComparableReadOnlyList<>).MakeGenericType(type.GetGenericArguments()[0]);
                return Activator.CreateInstance(type, list) ?? throw new Exception("could not create ComparableReadOnlyList");
            }

            var constructors = type.GetConstructors();
            var constructor = constructors.OrderByDescending(x => x.GetParameters().Length).First();
            var args = new List<object>();
            foreach (var p in constructor.GetParameters())
            {
                var typeToCreate = p.ParameterType;
                args.Add(Create(typeToCreate, r, stack.Add(type)));
            }
            return constructor.Invoke(args.ToArray());

            throw new NotSupportedException("Unsupported type: " + type.Name);
        }

        private static Type[] GetPossibleTypes(Type type)
        {
            if (type == typeof(Model.ExpressionSyntaxNode))
                return new[] { typeof(Model.MissingExpressionSyntaxNode) };

            var allTypes = type.Assembly.GetTypes();
            var list = new List<Type>();
            foreach (var t in allTypes.Where(x => x.IsClass))
            {
                if (t.IsAssignableTo(type) && !t.IsAbstract)
                    list.Add(t);
            }
            return list.ToArray();
        }
    }
}