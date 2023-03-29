// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

using static LanguageVersion;

/// <summary>
///   Extension methods to interpret <see cref="LanguageVersion"/>.
/// </summary>
internal static class LanguageFacts
{
    internal static bool SupportsFileScopedNamespaces(this LanguageVersion version)
        => version >= CSharp10;

    internal static bool SupportsNullableAnalysis(this LanguageVersion version)
        => version >= CSharp8;
}
