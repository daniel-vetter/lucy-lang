using Lucy.Core.Parsing;

namespace Lucy.Core.Tests
{
    public class EqualityTests
    {
        [SetUp]
        public void Setup()
        {
        }

        public static System.Collections.IEnumerable GetEqualTestCases()
        {
            return GetTypes().Select(x => new TestCaseData(x).SetName(x.Name));
        }

        private static List<Type> GetTypes()
        {
            var list = new List<Type>();
            var allTypesInAssembly = typeof(SyntaxTreeNode).Assembly.GetTypes();
            var allConsideredType = allTypesInAssembly
                .Where(x => x.IsAssignableTo(typeof(SyntaxTreeNode)))
                .Where(x => !x.IsAbstract);

            foreach (var type in allConsideredType)
                list.Add(type);
            return list;
        }

        [TestCaseSource(nameof(GetEqualTestCases))]
        public void CheckEqualImplemenationIsValid(Type type)
        {
            for (int i = 1; i <= 10; i += 2)
            {
                var errors = new List<string>();

                /*
                var equalsMethod = type.GetMethod("Equals");
                if (equalsMethod == null || equalsMethod.DeclaringType != type)
                    errors.Add("Type does not override the \"Equals\" method.");
                */
                var original = Generator.Create(type, i);
                var same = Generator.Create(type, i);
                var different = Generator.Create(type, i+1);

                if (!original.Equals(same))
                    errors.Add("Calling Equals on equal objects returned false.");

                if (original.GetHashCode() != same.GetHashCode())
                    errors.Add("Hashcode of equal objects was not the same.");

                if (original.Equals(different))
                    errors.Add("Calling Equals on non equal objects returned true.");

                if (original.GetHashCode() == different.GetHashCode())
                    errors.Add("Hashcode of not equal objects was the same.");

                if (errors.Any())
                    Assert.Fail($"Error in iteration {i}\n{string.Join("\n", errors)}");
            }
        }
    }
}