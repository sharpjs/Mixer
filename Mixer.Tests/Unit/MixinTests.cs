// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class MixinTests
{
    // GUIDANCE: Prefer integration tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void Construct_NullType()
    {
        Invoking(() => new Mixin(null!, ClassDeclaration("C"), default))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("type");
    }

    [Test]
    public void Construct_NullContent()
    {
        Invoking(() => new Mixin(WithType(), null!, default))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("content");
    }

    [Test]
    public void Construct_ClosedGenericType()
    {
        Invoking(() => new Mixin(WithClosedGenericType(), ClassDeclaration("C"), default))
            .Should().ThrowExactly<ArgumentException>()
            .WithParameterName("type")
            .WithMessage("Type must be a non-generic or open generic type*");
    }

    [Test]
    public void Close_NullType()
    {
        new Mixin(WithType(), ClassDeclaration("C"), default)
            .Invoking(m => m.Close(null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("type");
    }

    [Test]
    public void Close_OpenGenericType()
    {
        new Mixin(WithType(), ClassDeclaration("C"), default)
            .Invoking(m => m.Close(WithOpenGenericType()))
            .Should().ThrowExactly<ArgumentException>()
            .WithMessage("Type must be a non-generic or closed generic type*")
            .WithParameterName("type");
    }

    [Test]
    public void Close_NotConstructedFromMixinType()
    {
        new Mixin(WithType(), ClassDeclaration("C"), default)
            .Invoking(m => m.Close(WithClosedGenericType())) // not constructed from mixin type
            .Should().ThrowExactly<ArgumentException>()
            .WithMessage("Type must be constructed from the mixin type*")
            .WithParameterName("type");
    }

    private static INamedTypeSymbol WithOpenGenericType()
    {
        return WithType(
            argumentKinds: SymbolKind.TypeParameter
        );
    }

    private static INamedTypeSymbol WithClosedGenericType()
    {
        return WithType(
            constructedFrom: WithOpenGenericType(),
            argumentKinds:   SymbolKind.NamedType
        );
    }

    private static INamedTypeSymbol WithType(params SymbolKind[] argumentKinds)
    {
        return WithType(constructedFrom: null, argumentKinds);
    }

    private static INamedTypeSymbol WithType(
        INamedTypeSymbol?   constructedFrom,
        params SymbolKind[] argumentKinds)
    {
        var type = new Mock<INamedTypeSymbol>();

        type.Setup(t => t.TypeArguments)
            .Returns(ImmutableArray.CreateRange(argumentKinds.ToImmutableArray(), WithTypeArgument));

        type.Setup(t => t.ConstructedFrom)
            .Returns(constructedFrom ?? type.Object);

        return type.Object;
    }

    private static ITypeSymbol WithTypeArgument(SymbolKind kind)
    {
        return Mock.Of<ITypeSymbol>(a => a.Kind == kind);
    }
}
