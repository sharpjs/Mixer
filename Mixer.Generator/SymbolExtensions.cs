// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

using static SpecialType;
using static SymbolKind;

/// <summary>
///   Extension methods for <see cref="ISymbol"/> and derived types.
/// </summary>
internal static class SymbolExtensions
{
    // Inheritance Hierarchy
    // =====================
    //
    // INamespaceOrTypeSymbol
    // - INamespaceSymbol
    // - ITypeSymbol 
    //   - IArrayTypeSymbol
    //   - IDynamicTypeSymbol
    //   - IFunctionPointerTypeSymbol
    //   - INamedTypeSymbol
    //     - IErrorTypeSymbol
    //   - IPointerTypeSymbol
    //   - ITypeParameterSymbol

    private static StringBuilder AppendForFileName(this StringBuilder builder, INamespaceSymbol ns)
    {
        if (ns.ContainingSymbol is INamespaceSymbol parent && !parent.IsGlobalNamespace)
            builder.AppendForFileName(parent).Append('.');

        return builder.Append(ns.Name);
    }

    private static StringBuilder AppendForFileName(this StringBuilder builder, ITypeSymbol type)
    {
        return type.Kind switch
        {
            NamedType           => builder.AppendForFileName((INamedTypeSymbol) type),
            ArrayType           => builder.AppendForFileName((IArrayTypeSymbol) type),
            TypeParameter       => builder.Append(type.Name),
            DynamicType         => builder.Append(type.Name),
            _                   => builder.Append("ERROR"), // Pointers cannot be type arguments
        };
    }

    public static StringBuilder AppendForFileName(this StringBuilder builder, INamedTypeSymbol type)
    {
        if (type.HasShortName(out var name))
            return builder.Append(name);

        if (type.IsNullableOfT(out var t))
            return builder.AppendForFileName(t).Append('?');

        return builder.AppendForFileNameCore(type);
    }

    private static StringBuilder AppendForFileNameCore(this StringBuilder builder, INamedTypeSymbol type)
    {
        switch (type.ContainingSymbol)
        {
            case INamedTypeSymbol parent:
                builder.AppendForFileNameCore(parent).Append('.');
                break;

            case INamespaceSymbol ns when !ns.IsGlobalNamespace:
                builder.AppendForFileName(ns).Append('.');
                break;

            // Skip for global namespace, module, or assembly
        }

        builder.Append(type.Name);

        if (type.Arity == 0)
            return builder;

        builder.Append('{');

        var hasPreceding = false;

        foreach (var argument in type.TypeArguments)
        {
            if (hasPreceding)
                builder.Append(",");
            else
                hasPreceding = true;

            builder.AppendForFileName(argument);
        }

        return builder.Append('}');
    }

    private static StringBuilder AppendForFileName(this StringBuilder builder, IArrayTypeSymbol type)
    {
        return builder
            .AppendForFileName(type.ElementType)
            .Append('[')
            .Append(',', type.Rank - 1)
            .Append(']');
    }

    private static bool HasShortName(
        this INamedTypeSymbol             type,
        [MaybeNullWhen(false)] out string name)
    {
        name = type.SpecialType switch
        {
            System_Object     => "object",
            System_Void       => "void",
            System_Boolean    => "bool",
            System_Char       => "char",
            System_SByte      => "sbyte",
            System_Byte       => "byte",
            System_Int16      => "short",
            System_UInt16     => "ushort",
            System_Int32      => "int",
            System_UInt32     => "uint",
            System_Int64      => "long",
            System_UInt64     => "ulong",
            System_Decimal    => "decimal",
            System_Single     => "single",
            System_Double     => "double",
            System_String     => "string",
            _                 => null,
        };

        return name is not null;
    }

    private static bool IsNullableOfT(
        this INamedTypeSymbol                  type,
        [MaybeNullWhen(false)] out ITypeSymbol innerType)
    {
        innerType = type.OriginalDefinition.SpecialType is System_Nullable_T
            ? type.TypeArguments[0]
            : null;

        return innerType is not null;
    }

    public static bool IsConstructedFrom(this INamedTypeSymbol type, INamedTypeSymbol other)
    {
        return SymbolEqualityComparer.IncludeNullability
            .Equals(type.OriginalDefinition, other);

        // Cannot use type.ConstructedFrom here, becuase when type is a generic
        // nested within a generic, ConstructedFrom returns a partially closed
        // type (closed outer, open inner).
    }

    public static bool HasTypeParameters(this INamedTypeSymbol type)
    {
        foreach (var argument in type.TypeArguments)
            if (argument.Kind == TypeParameter)
                return true;

        return false;
    }

    public static bool HasTypeParameterSubstitutions(this INamedTypeSymbol type)
    {
        foreach (var argument in type.TypeArguments)
            if (argument.Kind != TypeParameter)
                return true;

        return false;
    }

    public static bool HasConstraints(this ITypeParameterSymbol parameter)
    {
        return parameter.HasSpecialPrimaryConstraint()  // primary special
            || parameter.ConstraintTypes.Any()          // primary class, secondary
            || parameter.HasConstructorConstraint;      // constructor
    }

    public static bool HasSpecialPrimaryConstraint(this ITypeParameterSymbol parameter)
    {
        return parameter.HasReferenceTypeConstraint     // class
            || parameter.HasValueTypeConstraint         // struct
            || parameter.HasNotNullConstraint           // notnull
            || parameter.HasUnmanagedTypeConstraint;    // unmanaged
    }
}
