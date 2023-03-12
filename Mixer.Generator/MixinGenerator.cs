// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

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
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Discover mixins: [Mixin]
        var mixins = context.SyntaxProvider
            .ForAttributeWithMetadataName(MixinAttributeName, IsSupported, CaptureMixin)
            .Collect()
            .Select(ToDictionary);

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

    private static bool IsSupported(SyntaxNode node, CancellationToken cancellation)
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
        var node = (TypeDeclarationSyntax) context.TargetNode;
        var type = (INamedTypeSymbol)      context.TargetSymbol;

        node = new MixinGeneralizer(type, context.SemanticModel, cancellation).Generalize(node);

        return new(type, node);
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
        var references = context.Attributes.GetMixinReferences0();

        return CaptureInclusion(ref context, references, isGeneric: false);
    }

    private static Inclusion CaptureInclusion1(
        GeneratorAttributeSyntaxContext context,
        CancellationToken               cancellation)
    {
        var references = context.Attributes.GetMixinReferences1();

        return CaptureInclusion(ref context, references, isGeneric: true);
    }

    private static Inclusion CaptureInclusion(
        ref GeneratorAttributeSyntaxContext context,
        ImmutableArray<MixinReference>      references,
        bool                                isGeneric)
    {
        var node = (TypeDeclarationSyntax) context.TargetNode;
        var type = (INamedTypeSymbol)      context.TargetSymbol;

        var target = TargetTemplatizer.Templatize(type, node);

        return new Inclusion(references, target, isGeneric);
    }

    private static void Execute(
        SourceProductionContext      context,
        (Inclusion, MixinDictionary) input)
    {
        var (inclusion, mixinDictionary) = input;

        var mixins = inclusion.Mixins.Resolve(mixinDictionary, context.ReportDiagnostic);
        var output = new MixinOutputBuilder(inclusion.Target, mixins).Build();

        context.AddSource(inclusion.GetFileName(), output);
    }
}
