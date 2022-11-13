using Microsoft.CodeAnalysis;

namespace Lucy.Core.SourceGenerator
{
    [Generator]
    public class SyntaxTreeModelGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var files = context.AdditionalTextsProvider
                .Where(a => a.Path.EndsWith("model.xml"))
                .Select((a, c) => (name: Path.GetFileNameWithoutExtension(a.Path), content: a.GetText(c)!.ToString()));

            var compilationAndFiles = context.CompilationProvider.Combine(files.Collect());

            context.RegisterSourceOutput(compilationAndFiles, (productionContext, sourceContext) =>
            {
                
                foreach (var (name, configXml) in sourceContext.Right)
                {
                    var config = ConfigLoader.GetConfig(configXml);

                    ModelGenerator.Generate(productionContext, name, config);
                    FlatModelGenerator.Generate(productionContext, name, config);
                    ImmutableModelGenerator.Generate(productionContext, name, config);
                }
            });     
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
