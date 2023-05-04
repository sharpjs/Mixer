// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Unit;

[TestFixture]
public class SyntaxHelpersTests
{
    // GUIDANCE: Prefer functional tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void AddLeadingTrivia_Empty()
    {
        var nodes = ImmutableArray<SyntaxNode>.Empty;

        nodes.AddLeadingTrivia(TriviaList(Space)).Should().BeEquivalentTo(nodes);
    }
}
