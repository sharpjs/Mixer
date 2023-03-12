// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   A C# syntax rewriter that converts a mixin from the generalized form
///   produced by <see cref="MixinGeneralizer"/> to a specialized form ready
///   for inclusion into a specific target.
/// </summary>
/// <remarks>
///   <list type="bullet">
///     <item>
///         Replaces the target type placeholder <c>$This</c> with the target
///         type identifier in constructors and destructuors.
///     </item>
///     <item>
///         Replaces the target type placeholder <c>$This</c> with the fully
///         qualified target type name everywhere else.
///     </item>
///     <item>
///         Replaces type parameter placeholders <c>$T0</c>, <c>$T1</c>, … with
///         type arguments.
///     </item>
///   </list>
/// </remarks>
internal class MixinSpecializer : CSharpSyntaxRewriter
{
    private readonly INamedTypeSymbol                        _targetType;
    private /*lazy*/ SimpleNameSyntax?                       _targetName;
    private readonly ImmutableDictionary<string, TypeSyntax> _replacements;

    public MixinSpecializer(INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
    {
        if (sourceType is null)
            throw new ArgumentNullException(nameof(sourceType));
        if (targetType is null)
            throw new ArgumentNullException(nameof(targetType));

        _targetType   = targetType;
        _replacements = GenerateReplacements(sourceType);
    }

    private SimpleNameSyntax TargetName
        => _targetName ??= GetNameWithGenericArguments(_targetType);

    public TypeDeclarationSyntax Specialize(TypeDeclarationSyntax node)
    {
        if (node is null)
            throw new ArgumentNullException(nameof(node));

        return (TypeDeclarationSyntax) Visit(node)!;
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        node = (ConstructorDeclarationSyntax) base.VisitConstructorDeclaration(node)!;

        if (node.Identifier.ValueText == TargetTypeNamePlaceholder)
            node = node.WithIdentifier(Identifier(_targetType.Name));

        return node;
    }

    public override SyntaxNode? VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        node = (DestructorDeclarationSyntax) base.VisitDestructorDeclaration(node)!;

        if (node.Identifier.ValueText == TargetTypeNamePlaceholder)
            node = node.WithIdentifier(Identifier(_targetType.Name));

        return node;
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        return GetReplacement(node.Identifier.ValueText)
            ?? base.VisitIdentifierName(node);
    }

    private static ImmutableDictionary<string, TypeSyntax>
        GenerateReplacements(INamedTypeSymbol type)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, TypeSyntax>();

        GenerateReplacements(builder, type);

        return builder.ToImmutable();
    }

    private static void
        GenerateReplacements(
            ImmutableDictionary<string, TypeSyntax>.Builder dictionary,
            INamedTypeSymbol                                type
        )
    {
        if (type.ContainingType is { } parent)
            GenerateReplacements(dictionary, parent);

        foreach (var argument in type.TypeArguments)
            dictionary.Add(GetPlaceholder(dictionary.Count), Qualify(argument));
    }

    private TypeSyntax? GetReplacement(string name)
    {
        if (name == TargetTypeNamePlaceholder)
            return TargetName;

        if (_replacements.TryGetValue(name, out var replacement))
            return replacement;

        return null;
    }
}
