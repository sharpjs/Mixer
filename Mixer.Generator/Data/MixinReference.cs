// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   Represents a reference to a mixin within an inclusion request.
/// </summary>
internal readonly struct MixinReference
{
    /// <summary>
    ///   Initializes a new <see cref="MixinReference"/> instance.
    /// </summary>
    /// <param name="type">
    ///   The type of the referenced mixin.
    /// </param>
    /// <param name="site">
    ///   A reference to the syntax node of the mixin reference.
    /// </param>
    public MixinReference(INamedTypeSymbol type, SyntaxReference? site)
    {
        Type = type;
        Site = site;
    }

    /// <summary>
    ///   Gets the type of the referenced mixin.
    /// </summary>
    public INamedTypeSymbol Type { get; }

    /// <summary>
    ///   A reference to the syntax node of the mixin reference.
    /// </summary>
    private SyntaxReference? Site { get; }

    /// <summary>
    ///   Gets the code location of the mixin reference, or
    ///   <see langword="null"/> if the location is not known.
    /// </summary>
    public Location? GetLocation() => Site?.GetLocation();
}
