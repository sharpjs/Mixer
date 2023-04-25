// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

using static MixinReferenceHelpers;

using MixinDictionary = ImmutableDictionary<INamedTypeSymbol, Mixin>;

/// <summary>
///   An incremental generator that provides mixin support for C#.
/// </summary>
[Generator]
public class MixinGenerator : IIncrementalGenerator
{
    private const string
        MixinAttributeName    = "Mixer.MixinAttribute",
        IncludeAttribute0Name = "Mixer.IncludeAttribute",
        IncludeAttribute1Name = "Mixer.IncludeAttribute`1";

    public static IEqualityComparer<INamedTypeSymbol> TypeComparer
        => SymbolEqualityComparer.Default;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage] // Coverlet (erroneously?) thinks there are uncovered branches
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Discover language version
        var version = context.ParseOptionsProvider
            .Select(GetLanguageVersion);

        // Discover mixins: [Mixin]
        var mixins = context.SyntaxProvider
            .ForAttributeWithMetadataName(MixinAttributeName, IsSupported, CaptureMixin)
            .Collect()
            .Select(ToDictionary)
            .Combine(version);

        // Discover inclusions: [Include(typeof(T))]
        var inclusions0 = context.SyntaxProvider
            .ForAttributeWithMetadataName(IncludeAttribute0Name, IsSupported, CaptureInclusion0)
            .Combine(mixins);

        // Discover inclusions: [Include<T>]
        var inclusions1 = context.SyntaxProvider
            .ForAttributeWithMetadataName(IncludeAttribute1Name, IsSupported, CaptureInclusion1)
            .Combine(mixins);

        // Execute inclusions
        context.RegisterSourceOutput(inclusions0, Execute);
        context.RegisterSourceOutput(inclusions1, Execute);
    }

    internal static LanguageVersion GetLanguageVersion(ParseOptions options, CancellationToken _)
    {
        return options is CSharpParseOptions cs
            ? cs.LanguageVersion
            : LanguageVersion.Latest;
    }

    private static bool IsSupported(SyntaxNode node, CancellationToken _)
    {
        return node.RawKind is
        (
            (int) SyntaxKind.ClassDeclaration  or
            (int) SyntaxKind.StructDeclaration or
            (int) SyntaxKind.RecordDeclaration or
            (int) SyntaxKind.RecordStructDeclaration
        );
    }

    private static Mixin CaptureMixin(
        GeneratorAttributeSyntaxContext context,
        CancellationToken               cancellation)
    {
        var mixinType   = (INamedTypeSymbol)      context.TargetSymbol;
        var declaration = (TypeDeclarationSyntax) context.TargetNode;

        return new MixinGeneralizer(mixinType, context.SemanticModel, cancellation)
            .Generalize(declaration);
    }

    private static MixinDictionary ToDictionary(
        ImmutableArray<Mixin> mixins,
        CancellationToken     cancellation)
    {
        return mixins.ToImmutableDictionary(m => m.Type, TypeComparer);
    }

    private static Inclusion CaptureInclusion0(
        GeneratorAttributeSyntaxContext context,
        CancellationToken               cancellation)
    {
        var targetType = (INamedTypeSymbol) context.TargetSymbol;
        var references = GetMixinReferences0(context.Attributes);

        return new Inclusion(targetType, references, isGeneric: false);
    }

    private static Inclusion CaptureInclusion1(
        GeneratorAttributeSyntaxContext context,
        CancellationToken               cancellation)
    {
        var targetType = (INamedTypeSymbol) context.TargetSymbol;
        var references = GetMixinReferences1(context.Attributes);

        return new Inclusion(targetType, references, isGeneric: true);
    }

    private static void Execute(
        SourceProductionContext                         context,
        (Inclusion, (MixinDictionary, LanguageVersion)) item)
    {
        var (inclusion, (dictionary, version)) = item;

        var mixins = ResolveMixins(inclusion.Mixins, dictionary, context.ReportDiagnostic);
        if (!mixins.Any())
            return;

        var output = new MixinOutputBuilder(inclusion.TargetType, mixins, version).Build();

        context.AddSource(inclusion.GetFileName(), output);
    }
}
