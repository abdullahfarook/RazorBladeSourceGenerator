using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RazorBladeGenerator
{
    [Generator]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // Find all classes with [GenerateCode] attribute
            var provider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, _) => GetClassToTransform(ctx))
                .Where(static m => m is not null);

            // Combine with compilation
            var compilationAndClasses = context.CompilationProvider.Combine(provider.Collect());

            // Register the source generator
            context.RegisterSourceOutput(
                compilationAndClasses,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static ClassModel? GetClassToTransform(GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;

            // Check if class has [GenerateCode] attribute
            var hasAttribute = classDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .Any(attr => attr.Name.ToString() == "GenerateCode");

            if (!hasAttribute)
                return null;

            var semanticModel = context.SemanticModel;
            var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);

            if (symbol == null)
                return null;

            if (symbol is not Microsoft.CodeAnalysis.INamedTypeSymbol namedTypeSymbol)
                return null;

            return new ClassModel
            {
                Name = symbol.Name,
                Namespace = symbol.ContainingNamespace.ToDisplayString(),
                Properties = namedTypeSymbol.GetMembers()
                    .OfType<Microsoft.CodeAnalysis.IPropertySymbol>()
                    .Select(p => new PropertyModel
                    {
                        Name = p.Name,
                        Type = p.Type.ToDisplayString()
                    })
                    .ToList()
            };
        }

        private static void Execute(
            Compilation compilation,
            IEnumerable<ClassModel> classes,
            SourceProductionContext context)
        {
            if (!classes.Any())
                return;

            foreach (var classModel in classes)
            {
                try
                {
                    // Use RazorBlade template to generate code
                    // According to RazorBlade docs (https://github.com/ltrzesniewski/RazorBlade):
                    // - Templates with @inherits PlainTextTemplate<TModel> generate a constructor taking TModel
                    // - The base RazorTemplate class provides Render() method that returns string
                    // - Generated class name matches the .cshtml filename (ClassTemplate.cshtml -> ClassTemplate)
                    var template = new Templates.ClassTemplate(classModel);

                    // Render() is provided by RazorTemplate base class
                    // The generated ClassTemplate inherits from PlainTextTemplate<ClassModel>
                    // which inherits from RazorTemplate
                    string generatedCode = template.Render();

                    context.AddSource(
                        $"{classModel.Name}.g.cs",
                        SourceText.From(generatedCode, Encoding.UTF8, SourceHashAlgorithm.Sha256));
                }
                catch (System.Exception ex)
                {
                    // Report error if template rendering fails
                    context.ReportDiagnostic(Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "RB0001",
                            "Template rendering failed",
                            "Failed to render template for {0}: {1}",
                            "RazorBladeGenerator",
                            DiagnosticSeverity.Error,
                            isEnabledByDefault: true),
                        Location.None,
                        classModel.Name,
                        ex.Message));
                }
            }
        }
    }

    public class ClassModel
    {
        public string Name { get; set; } = string.Empty;
        public string Namespace { get; set; } = string.Empty;
        public List<PropertyModel> Properties { get; set; } = new();
    }

    public class PropertyModel
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
    }
}

