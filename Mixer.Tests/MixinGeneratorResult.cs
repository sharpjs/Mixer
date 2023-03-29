// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Collections.Immutable;

namespace Mixer;

internal readonly struct MixinGeneratorResult
{
    public ImmutableDictionary<string, string> GeneratedSources { get; }

    public ImmutableArray<Diagnostic> Diagnostics { get; }

    public MixinGeneratorResult(ImmutableArray<Diagnostic> diagnostics)
    {
        GeneratedSources = ImmutableDictionary.Create<string, string>();
        Diagnostics      = diagnostics;
    }

    public MixinGeneratorResult(ref GeneratorRunResult result)
    {
        GeneratedSources = result.GeneratedSources.ToImmutableDictionary(
            s => s.HintName,
            s => s.SourceText.ToString()
        );

        Diagnostics = result.Diagnostics;
    }
}
