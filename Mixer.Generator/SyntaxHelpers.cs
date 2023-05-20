// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Mixer.SyntaxHelpers;

using System.Runtime.InteropServices;

namespace Mixer;

/// <summary>
///   Global helper methods for <see cref="SyntaxNode"/> objects.
/// </summary>
internal static class SyntaxHelpers
{
    public const string TargetTypeNamePlaceholder = "$This";

    /// <summary>
    ///   Gets visual size of an indent level in characters.
    /// </summary>
    /// <remarks>
    ///   This code assumes that the developer prefers a virtual tab stop every
    ///   four columns indented with spaces, not tabs, as is common practice in
    ///   the C# community.  It is beyond the scope of this generator to honor
    ///   alternative preferences, per the guidance
    ///   <a href="https://stackoverflow.com/questions/67351269/can-a-roslyn-source-generator-discover-the-ides-spacing-etc-preferences">here</a>
    ///   and
    ///   <a href="https://github.com/dotnet/roslyn/issues/53020">here</a>.
    /// </remarks>
    public const int IndentSize = 4;

#if USED
    public static bool IsNone(this SyntaxTrivia trivia)
        => trivia.IsKind(SyntaxKind.None);
#endif

    public static bool IsSome(this SyntaxTrivia trivia)
        => !trivia.IsKind(SyntaxKind.None);

    private static IdentifierNameSyntax Global()
        => IdentifierName(Token(SyntaxKind.GlobalKeyword));

    public static AliasQualifiedNameSyntax Global(string name)
        => AliasQualifiedName(Global(), IdentifierName(name));

#if USED
    public static AliasQualifiedNameSyntax Global(string alias, string name)
        => AliasQualifiedName(IdentifierName(alias), IdentifierName(name));
#endif

    public static QualifiedNameSyntax Dot(this NameSyntax lhs, string name)
        => QualifiedName(lhs, IdentifierName(name));

    public static ExpressionSyntax QualifyAsExpression(
        ISymbol           symbol,
        SimpleNameSyntax? name  = null,
        string?           alias = "global")
    {
        var expression = symbol.HasPredefinedName(out var keyword)
            ? PredefinedType(Token(keyword))
            : QualifyCore(symbol, name, alias, avoidMemberAccess: false);

        return expression.WithTriviaFromIfNotNull(name);
    }

    public static NameSyntax QualifyAsName(
        ISymbol           symbol,
        SimpleNameSyntax? name  = null,
        string?           alias = "global")
    {
        // The symbol should be an INamespaceSymbol except for special cases in
        // which qualification via member access expression is not supported.
        // One such case is a generic type constraint: it supports TypeSyntax
        // but not ExpressionSyntax.

        return (NameSyntax)
            QualifyCore(symbol, name, alias, avoidMemberAccess: true)
            .WithTriviaFromIfNotNull(name);
    }

    private static ExpressionSyntax QualifyCore(
        ISymbol           symbol,
        SimpleNameSyntax? name,
        string?           alias,
        bool              avoidMemberAccess)
    {
        // NOTE: This method is not invoked in any context where a symbol could
        // have an empty name.

        // Ensure RHS identifier exists, is trivia-free, and is not a short name
        name = name is null || name.Identifier.Text != symbol.Name
            ? IdentifierName(symbol.Name) // (re)synthesize: Foo -> FooAttribute
            : name.WithoutTrivia();

        // Qualify
        return symbol.ContainingSymbol switch
        {
            INamespaceSymbol { IsGlobalNamespace: true } when alias is { Length: > 0 }
                => AliasQualifiedName(IdentifierName(alias), name),

            INamespaceSymbol { IsGlobalNamespace: true }
                => name,

            INamespaceSymbol parent
                => QualifiedName(QualifyAsName(parent, null, alias), name),

            var parent when avoidMemberAccess
                => QualifiedName(QualifyAsName(parent, null, alias), name),

            var parent
                => MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    QualifyAsExpression(parent, null, alias),
                    name
                )
        };
    }

    public static bool HasPredefinedName(this ISymbol? symbol, out SyntaxKind keyword)
    {
        if (symbol is ITypeSymbol type)
            return type.HasPredefinedName(out keyword);

        keyword = default;
        return false;
    }

    public static bool HasPredefinedName(this ITypeSymbol type, out SyntaxKind keyword)
    {
        keyword = type.SpecialType switch
        {
            SpecialType.System_Object  => SyntaxKind.ObjectKeyword,
            SpecialType.System_Boolean => SyntaxKind.BoolKeyword,
            SpecialType.System_Char    => SyntaxKind.CharKeyword,
            SpecialType.System_SByte   => SyntaxKind.SByteKeyword,
            SpecialType.System_Byte    => SyntaxKind.ByteKeyword,
            SpecialType.System_Int16   => SyntaxKind.ShortKeyword,
            SpecialType.System_UInt16  => SyntaxKind.UShortKeyword,
            SpecialType.System_Int32   => SyntaxKind.IntKeyword,
            SpecialType.System_UInt32  => SyntaxKind.UIntKeyword,
            SpecialType.System_Int64   => SyntaxKind.LongKeyword,
            SpecialType.System_UInt64  => SyntaxKind.ULongKeyword,
            SpecialType.System_Decimal => SyntaxKind.DecimalKeyword,
            SpecialType.System_Single  => SyntaxKind.FloatKeyword,
            SpecialType.System_Double  => SyntaxKind.DoubleKeyword,
            SpecialType.System_String  => SyntaxKind.StringKeyword,
            _                          => SyntaxKind.None,
        };

        return keyword is not SyntaxKind.None;
    }

    public static string GetPlaceholder(int index)
    {
        return index.ToString("$0", CultureInfo.InvariantCulture);
    }

    public static SimpleNameSyntax GetNameWithGenericArguments(INamedTypeSymbol type)
    {
        if (!type.IsGenericType)
            return IdentifierName(type.Name);

        var arguments = MakeCommaSeparatedList(type.TypeParameters, ToTypeArgument);

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

    [ExcludeFromCodeCoverage] // because a test run can exercise only one path
    public static SyntaxTrivia GetPlatformEndOfLine()
    {
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? CarriageReturnLineFeed
            : LineFeed;
    }

    public static SyntaxToken TokenAndSpace(SyntaxKind kind)
    {
        return Token(default, kind, OneSpace());
    }

    public static SyntaxTriviaList OneSpace()
    {
        return TriviaList(Space);
    }

    public static LiteralExpressionSyntax LiteralExpression(string value)
    {
        return SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            Literal(value)
        );
    }

    public static AttributeArgumentListSyntax AttributeArgumentList(
        params SyntaxNodeOrToken[] arguments)
    {
        return SyntaxFactory.AttributeArgumentList(
            SeparatedList<AttributeArgumentSyntax>(
                NodeOrTokenList(arguments)
            )
        );
    }

    public static SeparatedSyntaxList<TTarget> MakeCommaSeparatedList<TSource, TTarget>(
        ImmutableArray<TSource> source,
        Func<TSource, TTarget>  selector)
        where TTarget : SyntaxNode
    {
        var array = ImmutableArray.CreateBuilder<SyntaxNodeOrToken>(source.Length * 2 - 1);

        for (var index = 0;;)
        {
            array.Add(selector(source[index]));

            if (++index == source.Length)
                break;

            array.Add(TokenAndSpace(SyntaxKind.CommaToken));
        }

        return SeparatedList<TTarget>(array.MoveToImmutable());
    }

    public static void AddCommaSeparated(
        this ImmutableArray<SyntaxNodeOrToken>.Builder array,
        SyntaxNode                                     node)
    {
        if (array.Any())
            array.Add(TokenAndSpace(SyntaxKind.CommaToken));

        array.Add(node);
    }

    public static ArgumentListSyntax Prepend(
        this ArgumentListSyntax list,
        ArgumentSyntax          argument)
    {
        var arguments = list.Arguments;

        arguments = arguments.Any()
            ? SeparatedList<ArgumentSyntax>(
                arguments.GetWithSeparators().InsertRange(0, new SyntaxNodeOrToken[]
                {
                    argument,
                    TokenAndSpace(SyntaxKind.CommaToken)
                })
            )
            : arguments.Add(argument);

        return list.WithArguments(arguments);
    }

    public static T AddLeadingTrivia<T>(this T node, SyntaxTriviaList trivia)
        where T : SyntaxNode
    {
        return node.WithLeadingTrivia(
            trivia.AddRange(node.GetLeadingTrivia())
        );
    }

    public static ImmutableArray<T> AddLeadingTrivia<T>(
        this ImmutableArray<T> nodes,
        SyntaxTriviaList       trivia)
        where T : SyntaxNode
    {
        return nodes.Any()
            ? nodes.SetItem(0, nodes[0].AddLeadingTrivia(trivia))
            : nodes;
    }

    public static T WithTriviaFromIfNotNull<T>(this T node, SyntaxNode? other)
        where T : SyntaxNode
    {
        return other is null ? node : node.WithTriviaFrom(other);
    }

    /// <summary>
    ///   Creates a new <see langword="notnull"/> type constraint.
    /// </summary>
    public static TypeConstraintSyntax NotNullConstraint()
    {
        return TypeConstraint(IdentifierName("notnull"));
    }

    /// <summary>
    ///   Creates a new <see langword="unmanaged"/> type constraint.
    /// </summary>
    public static TypeConstraintSyntax UnmanagedConstraint()
    {
        return TypeConstraint(IdentifierName(
            Identifier(default, SyntaxKind.UnmanagedKeyword, "unmanaged", "unmanaged", default)
        ));
    }
}
