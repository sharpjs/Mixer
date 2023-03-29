// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   Represents a request to include a set of mixins into a target.
/// </summary>
internal class Inclusion
{
    /// <summary>
    ///   Initializes a new <see cref="Inclusion"/> instance.
    /// </summary>
    /// <param name="targetType">
    ///   The type into which to include <paramref name="mixins"/>.
    /// </param>
    /// <param name="mixins">
    ///   References to mixins to include into <paramref name="targetType"/>.
    /// </param>
    /// <param name="isGeneric">
    ///   <see langword="true"/> if the inclusion request arises via the
    ///     generic <c>IncludeAttribute&lt;T&gt;</c>;
    ///   <see langword="false"/> if the inclusion request arises via the
    ///     non-generic <c>IncludeAttribute</c>.
    /// </param>
    public Inclusion(
        INamedTypeSymbol               targetType,
        ImmutableArray<MixinReference> mixins,
        bool                           isGeneric)
    {
        TargetType = targetType;
        Mixins     = mixins;
        IsGeneric  = isGeneric;
    }

    /// <summary>
    ///   Gets the target type into which to include <see cref="Mixins"/>.
    /// </summary>
    public INamedTypeSymbol TargetType { get; }

    /// <summary>
    ///   Gets the references to mixins to include into <see cref="TargetType"/>.
    /// </summary>
    public ImmutableArray<MixinReference> Mixins { get; }

    /// <summary>
    ///   Gets whether the inclusion request arose via the generic
    ///   <c>IncludeAttribute&lt;T&gt;</c>.
    /// </summary>
    public bool IsGeneric { get; }

    /// <summary>
    ///   Gets the name of the file to generate for the inclusion.
    /// </summary>
    public string GetFileName()
    {
        // NOTE: The name allows any identifier character or .,-+`_ ()[]{}
        return new StringBuilder()
            .AppendForFileName(TargetType)
            .AppendFormat(IsGeneric ? ".1" : ".0")
            .Append(".g.cs")
            .ToString();
    }
}
