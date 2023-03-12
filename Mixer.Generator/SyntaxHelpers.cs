// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Mixer.SyntaxHelpers;

namespace Mixer;

/// <summary>
///   Global helper methods for <see cref="SyntaxNode"/> objects.
/// </summary>
internal static class SyntaxHelpers
{
    public const string TargetTypeNamePlaceholder = "$This";

    private static IdentifierNameSyntax Global()
        => IdentifierName(Token(SyntaxKind.GlobalKeyword));

    public static AliasQualifiedNameSyntax Global(string name)
        => AliasQualifiedName(Global(), IdentifierName(name));

    public static AliasQualifiedNameSyntax Global(string alias, string name)
        => AliasQualifiedName(IdentifierName(alias), IdentifierName(name));

    public static QualifiedNameSyntax Dot(this NameSyntax lhs, string name)
        => QualifiedName(lhs, IdentifierName(name));

    public static NameSyntax Qualify(ISymbol symbol, string? alias = "global")
    // <: AliasQualifiedNameSyntax | QualifiedNameSyntax
    {
        return Qualify(symbol, IdentifierName(symbol.Name), alias);
    }

    public static NameSyntax Qualify(ISymbol symbol, SimpleNameSyntax name, string? alias = "global")
    // <: AliasQualifiedNameSyntax | QualifiedNameSyntax
    {
        for (;;)
        {
            if (symbol.ContainingSymbol is not { } parent)
                return name;

            if (parent is INamespaceSymbol { IsGlobalNamespace: true })
                return alias is { Length: > 0 }
                    ? AliasQualifiedName(IdentifierName(alias), name)
                    : name;

            if (parent.Name.Length > 0)
                return QualifiedName(Qualify(parent, alias), name);

            // Skip parent with empty name
            symbol = parent;
        }
    }

    public static string GetPlaceholder(int index)
    {
        return index.ToString("$0", CultureInfo.InvariantCulture);
    }

    public static SimpleNameSyntax GetNameWithGenericArguments(INamedTypeSymbol type)
    {
        // TODO: Test edge cases here.

        if (!type.IsGenericType)
            return IdentifierName(type.Name);

        var arguments = SeparatedList(type.TypeParameters.Select(ToTypeArgument));

        return GenericName(Identifier(type.Name), TypeArgumentList(arguments));
    }

    private static TypeSyntax ToTypeArgument(ITypeParameterSymbol parameter)
    {
        return IdentifierName(parameter.Name);
    }

    /// <summary>
    ///   Gets the location of the specified syntax reference.
    /// </summary>
    /// <param name="reference">
    ///   The syntax reference from which to get the location.
    /// </param>
    /// <returns>
    ///   The location of <paramref name="reference"/>.
    /// </returns>
    /// <remarks>
    ///   <para>
    ///     More performant than <c>GetSyntax().GetLocation()</c>.
    ///   </para>
    ///   <para>
    ///     This method is present in the Roslyn API but internal.
    ///   </para>
    /// </remarks>
    public static Location GetLocation(this SyntaxReference reference)
    {
        return reference.SyntaxTree.GetLocation(reference.Span);
    }
}
