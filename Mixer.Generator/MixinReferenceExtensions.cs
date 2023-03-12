// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   Extension methods for discovering <see cref="MixinReference"/> instances
///   and for resolving them to <see cref="Mixin"/> instances.
/// </summary>
internal static class MixinReferenceExtensions
{
    public static ImmutableArray<MixinReference> GetMixinReferences0(
        this ImmutableArray<AttributeData> attributes)
    {
        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var builder = ImmutableArray.CreateBuilder<MixinReference>(attributes.Length);

        foreach (var attribute in attributes)
        {
            // [Include(typeof(SomeMixin))]

            if (attribute.AttributeClass is not { } attributeType)
                continue;

            if (attributeType.ContainingModule.Name != "Mixer.dll")
                continue;

            if (attributeType.Arity != 0)
                continue;

            if (attribute.ConstructorArguments.Length != 1)
                continue;

            var argument = attribute.ConstructorArguments[0];

            if (argument.Kind is not TypedConstantKind.Type)
                continue;

            if (argument.Value is not INamedTypeSymbol requestedType)
                continue;

            if (!visited.Add(requestedType))
                continue;

            builder.Add(new(requestedType, attribute.ApplicationSyntaxReference));
        }

        return builder.Count == builder.Capacity
            ? builder.MoveToImmutable()
            : builder.ToImmutable();
    }

    public static ImmutableArray<MixinReference> GetMixinReferences1(
        this ImmutableArray<AttributeData> attributes)
    {
        var visited = new HashSet<INamedTypeSymbol>(SymbolEqualityComparer.Default);
        var builder = ImmutableArray.CreateBuilder<MixinReference>(attributes.Length);

        foreach (var attribute in attributes)
        {
            // [Include<SomeMixin>]

            if (attribute.AttributeClass is not { } attributeType)
                continue;

            if (attributeType.ContainingModule.Name != "Mixer.dll")
                continue;

            if (attributeType.Arity != 1)
                continue;

            if (attributeType.TypeArguments[0] is not INamedTypeSymbol requestedType)
                continue;

            if (!visited.Add(requestedType))
                continue;

            builder.Add(new(requestedType, attribute.ApplicationSyntaxReference));
        }

        return builder.Count == builder.Capacity
            ? builder.MoveToImmutable()
            : builder.ToImmutable();
    }

    public static ImmutableArray<Mixin> Resolve(
        this ImmutableArray <MixinReference>          references,
        ImmutableDictionary <INamedTypeSymbol, Mixin> mixins,
        Action<Diagnostic>                            reporter)
    {
        if (mixins is null)
            throw new ArgumentNullException(nameof(mixins));
        if (reporter is null)
            throw new ArgumentNullException(nameof(reporter));

        var builder = ImmutableArray.CreateBuilder<Mixin>(references.Length);

        foreach (var reference in references)
        {
            if (mixins.TryGetValue(reference.Type.OriginalDefinition, out var mixin))
                builder.Add(mixin.Close(reference.Type));
            else
                reporter(Diagnostics.NotMixin(reference));
        }

        return builder.Count == builder.Capacity
            ? builder.MoveToImmutable()
            : builder.ToImmutable();
    }
}
