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
    /// <param name="mixins"></param>
    /// <param name="target"></param>
    /// <param name="isGeneric"></param>
    public Inclusion(ImmutableArray<MixinReference> mixins, Target target, bool isGeneric)
    {
        Mixins    = mixins;
        Target    = target;
        IsGeneric = isGeneric;
    }

    /// <summary>
    ///   Gets the references to mixins to include into <see cref="Target"/>.
    /// </summary>
    public ImmutableArray<MixinReference> Mixins { get; }

    /// <summary>
    ///   Gets the target into which to include <see cref="Mixins"/>.
    /// </summary>
    public Target Target { get; }

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
            .AppendForFileName(Target.Type)
            .AppendFormat(IsGeneric ? ".1" : ".0")
            .Append(".g.cs")
            .ToString();
    }
}
