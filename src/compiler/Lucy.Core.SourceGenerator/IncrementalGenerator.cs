using Lucy.Core.SourceGenerator.Generators;
using Lucy.Core.SourceGenerator.Infrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Lucy.Core.SourceGenerator
{
    [Generator]
    public class IncrementalGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            CreateModel(context);
            DbExtensionMethodGenerator.Register(context);
        }

        private static void CreateModel(IncrementalGeneratorInitializationContext context)
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

                    SyntaxTreeModelBuilderGenerator.Generate(productionContext, config);
                    SyntaxTreeModelGenerator.Generate(productionContext, config);
                }
            });
        }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}
