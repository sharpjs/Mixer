// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Unit;

[TestFixture]
public class MixinReferenceTests
{
    // GUIDANCE: Prefer functional tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void GetLocation_NullSite()
    {
        new MixinReference(Mock.Of<INamedTypeSymbol>(), site: null)
            .GetLocation().Should().BeNull();
    }
}
