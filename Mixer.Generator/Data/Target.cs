// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   Represents a target type into which mixins can be included.
/// </summary>
internal readonly struct Target
{
    /// <summary>
    ///   Initializes a new <see cref="Target"/> instance.
    /// </summary>
    /// <param name="type">
    ///   The type of the target.
    ///   Must be a non-generic or <b>open</b> generic type.
    /// </param>
    /// <param name="content">
    ///   The content of the target.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="type"/> or <paramref name="content"/> is
    ///   <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    ///   <paramref name="type"/> is a partially or fully closed generic type.
    /// </exception>
    public Target(INamedTypeSymbol type, CompilationUnitSyntax content)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        if (content is null)
            throw new ArgumentNullException(nameof(content));
        if (type.HasTypeParameterSubstitutions())
            throw new ArgumentException("Type must be a non-generic or open generic type.", nameof(type));

        Type             = type;
        ContentReference = content.GetReference();
    }

    /// <summary>
    ///   Gets the type of the target.
    ///   A non-generic or <b>open</b> generic type.
    /// </summary>
    ///     <item>
    ///     </item>
    public INamedTypeSymbol Type { get; }

    /// <summary>
    ///   Gets a reference to the content of the target.
    /// </summary>
    private SyntaxReference ContentReference { get; }

    /// <summary>
    ///   Gets the content of the target.
    /// </summary>
    /// <param name="cancellation">
    ///   A token to monitor for cancellation requests.
    /// </param>
    /// <returns>
    ///   The content of the target.
    /// </returns>
    /// <remarks>
    ///   This action might cause a parse to happen to recover the syntax node.
    /// </remarks>
    public CompilationUnitSyntax GetContent(CancellationToken cancellation)
        => (CompilationUnitSyntax) ContentReference.GetSyntax(cancellation);
}
