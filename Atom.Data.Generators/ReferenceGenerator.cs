using System.Collections.Immutable;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Scriban;

namespace Genius.Atom.Data.Generators;

[Generator]
public sealed class ReferenceGenerator : IIncrementalGenerator
{
    private const string IReferenceFullName = "Genius.Atom.Data.IReference";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Find all record declarations that implement IReference<T>
        var referenceDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsReferenceCandidate(node),
                transform: static (context, _) => GetReferenceInfo(context))
            .Where(static info => info is not null)
            .Select(static (info, _) => info!.Value);

        // Collect all references
        var collectedReferences = referenceDeclarations.Collect();

        // Generate the source
        context.RegisterSourceOutput(collectedReferences, static (spc, references) => Execute(spc, references));
    }

    private static bool IsReferenceCandidate(SyntaxNode node)
    {
        // Look for record declarations with base types
        return node is RecordDeclarationSyntax recordDecl
            && recordDecl.BaseList?.Types.Count > 0;
    }

    private static ReferenceInfo? GetReferenceInfo(GeneratorSyntaxContext context)
    {
        var recordDecl = (RecordDeclarationSyntax)context.Node;

        // Get the semantic model
        var semanticModel = context.SemanticModel;
        var symbol = semanticModel.GetDeclaredSymbol(recordDecl) as INamedTypeSymbol;

        if (symbol is null)
            return null;

        // Check if it implements IReference<TKey, TReference>
        foreach (var @interface in symbol.AllInterfaces)
        {
            // Get the unbound generic name (IReference<TKey, TReference> -> IReference)
            var unboundTypeName = @interface.OriginalDefinition.ContainingNamespace.ToDisplayString()
                + "." + @interface.OriginalDefinition.Name;

            if (unboundTypeName == IReferenceFullName && @interface.TypeArguments.Length == 2)
            {
                var namespaceName = symbol.ContainingNamespace.ToDisplayString();
                var className = symbol.Name;
                var keyType = @interface.TypeArguments[0].ToDisplayString(
                    SymbolDisplayFormat.MinimallyQualifiedFormat);

                return new ReferenceInfo(namespaceName, className, keyType);
            }
        }

        return null;
    }

    private static void Execute(SourceProductionContext context, ImmutableArray<ReferenceInfo> references)
    {
        if (references.IsDefaultOrEmpty)
            return;

        var template = LoadTemplate();

        foreach (var reference in references.Distinct())
        {
            var source = template.Render(new
            {
                @namespace = reference.Namespace,
                class_name = reference.ClassName,
                key_type = reference.KeyType
            });

            context.AddSource($"{reference.ClassName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }
    }

    private static Template LoadTemplate()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Genius.Atom.Data.Generators.Templates.ReferenceGenerated.scriban";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }

        using var reader = new System.IO.StreamReader(stream);
        var templateContent = reader.ReadToEnd();

        return Template.Parse(templateContent);
    }

    private readonly record struct ReferenceInfo(string Namespace, string ClassName, string KeyType);
}
