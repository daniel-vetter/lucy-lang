using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Lucy.Core.SemanticAnalysis.Infrastructure.Salsa
{
    public sealed class QueryGroupCollection
    {
        private readonly ImmutableList<Type> _types = ImmutableList<Type>.Empty;

        public QueryGroupCollection()
        {
        }

        private QueryGroupCollection(ImmutableList<Type> types)
        {
            _types = types;
        }

        public QueryGroupCollection Add<T>() => Add(typeof(T));
        public QueryGroupCollection Add(Type type) => new(_types.Add(type));

        public QueryGroupCollection AddFromCurrentAssembly() => AddFromAssembly(Assembly.GetCallingAssembly());

        public QueryGroupCollection AddFromAssembly(Assembly assembly)
        {
            var list = new List<Type>();
            foreach (var type in assembly.GetTypes())
            {
                if (type.GetCustomAttribute<QueryGroupAttribute>() == null)
                    continue;

                list.Add(type);
            }

            return new QueryGroupCollection(_types.AddRange(list));
        }

        public ImmutableList<Type> Types => _types;

        public IEnumerable<string> Validate()
        {
            var result = new List<string>();

            result.AddRange(ValidateGeneral());
            result.AddRange(ValidateDuplicates());
            result.AddRange(ValidateConstructor());
            result.AddRange(ValidateConstructorParameters());
            result.AddRange(ValidateCycles());

            return result;
        }

        private IEnumerable<string> ValidateCycles()
        {
            var paths = new List<Type[]>();
            
            foreach (var type in _types) 
                Traverse(type, ImmutableList<Type>.Empty, ImmutableHashSet<Type>.Empty);

            void Traverse(Type type, ImmutableList<Type> stackOrdered, ImmutableHashSet<Type> stack)
            {
                if (stack.Contains(type))
                {
                    paths.Add(stackOrdered
                        .SkipWhile(x => x != type)
                        .ToArray()
                    );
                    return;
                }
                
                foreach (var parameter in type.GetConstructors().FirstOrDefault()?.GetParameters() ?? Array.Empty<ParameterInfo>())
                {
                    Traverse(parameter.ParameterType, stackOrdered.Add(type), stack.Add(type));
                }
            }

            return paths
                .DistinctBy(x => string.Join(">", x.OrderBy(y => y.Name)))
                .Select(x => "Cycle detected: " + string.Join(" -> ", x.Append(x.First()).Select(y => y.Name)));
        }

        private IEnumerable<string> ValidateConstructorParameters()
        {
            var knownTypes = Types.ToHashSet();

            return Types
                .Select(x => x.GetConstructors())
                .Where(x => x.Length == 1)
                .Select(x => x[0])
                .SelectMany(x => x.GetParameters())
                .Where(x => !knownTypes.Contains(x.ParameterType))
                .Select(x => x + " depends on " + x.ParameterType + " which was not registered.");
        }

        private IEnumerable<string> ValidateConstructor()
        {
            foreach (var type in Types)
            {
                var ctr = type.GetConstructors();
                switch (ctr.Length)
                {
                    case > 1:
                        yield return "More than one constructor found in " + type;
                        break;
                    case 0:
                        yield return "No public constructor found in " + type;
                        break;
                }
            }
        }

        private IEnumerable<string> ValidateGeneral()
        {
            if (Types.Count == 0)
                yield return "No query groups registered.";

            foreach (var type in _types)
                if (!type.IsClass)
                    yield return type + " is not a valid type";
        }

        private IEnumerable<string> ValidateDuplicates()
        {
            foreach (var type in _types.GroupBy(x => x).Where(x => x.Count() > 1))
            {
                yield return type + " was registered more than once.";
            }
        }

        public void ThrowIfInvalid()
        {
            var errors = Validate().ToArray();
            if (errors.Any())
                throw new Exception("Invalid query group registration:\n" + string.Join("\n", errors));
        }
    }
    
    public class InjectAttribute : Attribute
    {
    }
    
    [AttributeUsage(AttributeTargets.Class)]
    [MeansImplicitUse]
    public class QueryGroupAttribute : Attribute
    {
    }
}