// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   Represents a type whose attributes, bases, and members are eligible for
///   inclusion in a target type.
/// </summary>
internal readonly struct Mixin
{
    /// <summary>
    ///   Initializes a new, open <see cref="Mixin"/> instance.
    /// </summary>
    /// <param name="type">
    ///   The type of the mixin.
    ///   Must be a non-generic or <b>open</b> generic type.
    /// </param>
    /// <param name="content">
    ///   The content of the mixin.
    /// </param>
    /// <param name="nullableContext">
    ///   The nullable analysis state for the mixin.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="type"/> or <paramref name="content"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="type"/> is a partially or fully closed generic type.
    /// </exception>
    public Mixin(
        INamedTypeSymbol      type,
        TypeDeclarationSyntax content,
        NullableContext       nullableContext)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (content is null)
            throw new ArgumentNullException(nameof(content));
        if (type.HasTypeParameterSubstitutions())
            throw new ArgumentException("Type must be a non-generic or open generic type.", nameof(type));

        Type             = type;
        ContentReference = content.GetReference();
        NullableContext  = nullableContext;
    }

    // Used by Close()
    private Mixin(INamedTypeSymbol type, SyntaxReference content, NullableContext nullableContext)
    {
        Type             = type;
        ContentReference = content;
        NullableContext  = nullableContext;
    }

    /// <summary>
    ///   Gets the type of the mixin.
    /// </summary>
    /// <remarks>
    ///   <list type="bullet">
    ///     <item>
    ///       For <b>open</b> mixins: a non-generic or <b>open</b> generic
    ///       type.
    ///     </item>
    ///     <item>
    ///       For <b>closed</b> mixins: a non-generic or <b>closed</b> generic
    ///       type.
    ///     </item>
    ///   </list>
    /// </remarks>
    public INamedTypeSymbol Type { get; }

    /// <summary>
    ///   Gets a reference to the content of the mixin.
    /// </summary>
    private SyntaxReference ContentReference { get; }

    /// <summary>
    ///   Gets the nullable analysis state for the mixin.
    /// </summary>
    public NullableContext NullableContext { get; }

    /// <summary>
    ///   Creates a new, closed <see cref="Mixin"/> instance from the current
    ///   instance.
    /// </summary>
    /// <param name="type">
    ///   <para>
    ///     The type of the closed mixin.
    ///   </para>
    ///   <para>
    ///     If non-generic, must be equal to the <see cref="Type"/> of the
    ///     current instance; otherwise, must be fully <b>closed</b> generic
    ///     type constructed from the <see cref="Type"/> of this instance.
    ///   </para>
    /// </param>
    /// <returns>
    ///   A new, closed <see cref="Mixin"/> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///     <paramref name="type"/> is a non-generic type not equal to
    ///     <see cref="Type"/>; or,
    ///   </para>
    ///   <para>
    ///     <paramref name="type"/> is a closed generic type not constructed
    ///     from <see cref="Type"/>; or,
    ///   </para>
    ///   <para>
    ///     <paramref name="type"/> is an open or partially closed generic
    ///     type.
    ///   </para>
    /// </exception>
    public Mixin Close(INamedTypeSymbol type)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (type.HasTypeParameters())
            throw new ArgumentException("Type must be a non-generic or closed generic type.", nameof(type));
        if (!type.IsConstructedFrom(Type))
            throw new ArgumentException("Type must be constructed from from the mixin type.", nameof(type));

        return new(type, ContentReference, NullableContext);
    }

    /// <summary>
    ///   Gets the content of the mixin.
    /// </summary>
    /// <param name="cancellation">
    ///   A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///   The content of the mixin.
    /// </returns>
    /// <remarks>
    ///   This action might cause a parse to happen to recover the syntax node.
    /// </remarks>
    public TypeDeclarationSyntax GetContent(CancellationToken cancellation)
        => (TypeDeclarationSyntax) ContentReference.GetSyntax(cancellation);
}
