// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

#pragma warning disable IDE1006 // Naming Styles
// Done to disambiguate diagnostic descriptors from factory methods.

namespace Mixer;

/// <summary>
///   Methods to create <see cref="Diagnostic"/> instances.
/// </summary>
internal static class Diagnostics
{
    private const string
        Category = "Mixer";

    /// <summary>
    ///   Creates a diagnostic to report that a mixin reference specifies a
    ///   non-mixin type.
    /// </summary>
    /// <param name="reference">
    ///   The mixin reference that specifies a non-mixin type.
    /// </param>
    /// <returns>
    ///   MIX0001: Included types must be mixins
    /// </returns>
    public static Diagnostic NotMixin(MixinReference reference)
        => Diagnostic.Create(_NotMixin, reference.GetLocation(), reference.Type);

    private static readonly DiagnosticDescriptor _NotMixin = new(
        id:                 "MIX0001",
        category:           Category,
        title:              "Included types must be mixins",
        messageFormat:      "Cannot include '{0}' because it is not a mixin. Mixins must be marked with [Mixin].",
        defaultSeverity:    DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}
