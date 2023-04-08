// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

#pragma warning disable IDE1006 // Naming Styles
// Done to disambiguate diagnostic descriptors from factory methods.

using Mixer.Properties;

namespace Mixer;

using static Resources;

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
        title:              Resource(nameof(MIX0001_Title)),
        messageFormat:      Resource(nameof(MIX0001_MessageFormat)),
        defaultSeverity:    DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    private static LocalizableResourceString Resource(string name)
        => new LocalizableResourceString(name, ResourceManager, typeof(Resources));
}
