// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Unit;

[TestFixture]
public class MixinGeneratorTests
{
    // GUIDANCE: Prefer functional tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void GetLanguageVersion_NotCSharpParseOptions()
    {
        MixinGenerator.GetLanguageVersion(options: null!, default)
            .Should().Be(LanguageVersion.Latest);
    }
}
