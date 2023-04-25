// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Unit;

[TestFixture]
public class SpaceNormalizerTests
{
    // GUIDANCE: Prefer functional tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void Normalize_Node_NegativeIndent()
    {
        new SpaceNormalizer()
            .Invoking(n => n.Normalize(EmptyStatement(), -1))
            .Should().Throw<ArgumentOutOfRangeException>();
    }

    [Test]
    public void Normalize_List_NegativeIndent()
    {
        new SpaceNormalizer()
            .Invoking(n => n.Normalize(SingletonList(EmptyStatement()), -1))
            .Should().Throw<ArgumentOutOfRangeException>();
    }
}
