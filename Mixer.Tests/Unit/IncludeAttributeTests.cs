// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class IncludeAttributeTests
{
    // GUIDANCE: Prefer integration tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void Nongeneric_Construct_NullType()
    {
        Invoking(() => new IncludeAttribute(null!))
            .Should().Throw<ArgumentNullException>().WithParameterName("type");
    }

    [Test]
    public void Nongeneric_Type_Get()
    {
        new IncludeAttribute(typeof(string)).Type.Should().Be(typeof(string));
    }

    [Test]
    public void Gneric_Type_Get()
    {
        new IncludeAttribute<string>().Type.Should().Be(typeof(string));
    }
}
