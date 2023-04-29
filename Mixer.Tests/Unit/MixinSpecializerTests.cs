// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Unit;

[TestFixture]
public class MixinSpecializerTests
{
    // GUIDANCE: Prefer functional tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void Construct_NullMixinType()
    {
        Invoking(() => new MixinSpecializer(null!, Mock.Of<INamedTypeSymbol>()))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Construct_NullTargetType()
    {
        Invoking(() => new MixinSpecializer(Mock.Of<INamedTypeSymbol>(), null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Specialize_NullDeclaration()
    {
        var mixinType  = Mock.Of<INamedTypeSymbol>();
        var targetType = Mock.Of<INamedTypeSymbol>();

        Mock.Get(mixinType)
            .Setup(t => t.TypeArguments)
            .Returns(ImmutableArray.Create<ITypeSymbol>());

        new MixinSpecializer(mixinType, targetType)
            .Invoking(s => s.Specialize(null!))
            .Should().Throw<ArgumentNullException>();
    }
}
