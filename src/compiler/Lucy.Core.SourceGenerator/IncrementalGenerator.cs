using Lucy.Core.SourceGenerator.Generators;
using Microsoft.CodeAnalysis;

namespace Lucy.Core.SourceGenerator;

[Generator]
public class IncrementalGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        DbExtensionMethodGenerator.Register(context);
    }
}

internal static class IsExternalInit { }