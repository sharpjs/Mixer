// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   Information about the source generator.
/// </summary>
internal static class Tool
{
    /// <summary>
    ///   Gets the name of the source generator.
    /// </summary>
    public static string Name { get; }
        = typeof(MixinGenerator).Assembly.GetName().Name;

    /// <summary>
    ///   Gets the version of the source generator.
    /// </summary>
    public static Version Version { get; private set; }
        = typeof(MixinGenerator).Assembly.GetName().Version;

    internal static void SetZeroVersionForTesting()
        => Version = new Version(0, 0, 0, 0);
}
