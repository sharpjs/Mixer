// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Moq.Protected;

namespace Mixer.Tests.Unit;

[TestFixture]
public class MixinReferenceHelpersTests
{
    [Test]
    public void GetMixinReferences0_NullAttributeClass()
    {
        using var h = new TestHarness();

        h.SetUpAttributeType(null);

        MixinReferenceHelpers.GetMixinReferences0(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences0_NotInMixerDll()
    {
        using var h = new TestHarness();

        h.SetUpAttributeModuleName("Other.dll");

        MixinReferenceHelpers.GetMixinReferences0(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences0_UnexpectedArity()
    {
        using var h = new TestHarness();

        h.SetUpAttributeTypeArity(1);

        MixinReferenceHelpers.GetMixinReferences0(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences0_UnexpectedArgumentCount()
    {
        using var h = new TestHarness();

        h.SetUpAttributeArguments();

        MixinReferenceHelpers.GetMixinReferences0(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences0_UnexpectedArgumentType()
    {
        using var h = new TestHarness();

        h.SetUpAttributeArguments(MakeTypedConstant(TypedConstantKind.Primitive, ""));

        MixinReferenceHelpers.GetMixinReferences0(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences0_UnexpectedArgumentValue()
    {
        using var h = new TestHarness();

        h.SetUpAttributeArguments(MakeTypedConstant(TypedConstantKind.Type, null));

        MixinReferenceHelpers.GetMixinReferences0(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences1_NullAttributeClass()
    {
        using var h = new TestHarness();

        h.SetUpAttributeType(null);

        MixinReferenceHelpers.GetMixinReferences1(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences1_NotInMixerDll()
    {
        using var h = new TestHarness();

        h.SetUpAttributeModuleName("Other.dll");

        MixinReferenceHelpers.GetMixinReferences1(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences1_UnexpectedArity()
    {
        using var h = new TestHarness();

        h.SetUpAttributeTypeArity(0);

        MixinReferenceHelpers.GetMixinReferences1(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void GetMixinReferences1_UnexpectedTypeArgument()
    {
        using var h = new TestHarness();

        h.SetUpAttributeTypeArguments(Mock.Of<ITypeSymbol>());

        MixinReferenceHelpers.GetMixinReferences1(h.MakeAttributes()).Should().BeEmpty();
    }

    [Test]
    public void ResolveMixins_NullMixins()
    {
        Invoking(() => MixinReferenceHelpers.ResolveMixins(default, null!, d => { }))
            .Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void ResolveMixins_NullAction()
    {
        var dictionary = ImmutableDictionary<INamedTypeSymbol, Mixin>.Empty;

        Invoking(() => MixinReferenceHelpers.ResolveMixins(default, dictionary, null!))
            .Should().Throw<ArgumentNullException>();
    }

    private class TestHarness : TestHarnessBase
    {
        private Mock<AttributeData>?    _attributeData;
        private Mock<INamedTypeSymbol>? _attributeType;
        private Mock<IModuleSymbol>?    _attributeModule;
        private Mock<INamedTypeSymbol>? _requestedType;

        public Mock<AttributeData> AttributeData
            => _attributeData ??= Mocks.Create<AttributeData>();

        public Mock<INamedTypeSymbol> AttributeType
            => _attributeType ??= Mocks.Create<INamedTypeSymbol>();

        public Mock<IModuleSymbol> AttributeModule
            => _attributeModule ??= Mocks.Create<IModuleSymbol>();

        public Mock<INamedTypeSymbol> RequestedType
            => _requestedType ??= Mocks.Create<INamedTypeSymbol>();

        public ImmutableArray<AttributeData> MakeAttributes()
            => ImmutableArray.Create(AttributeData.Object);

        public void SetUpAttributeType(INamedTypeSymbol? type)
        {
            AttributeData
                .Protected()
                .Setup<INamedTypeSymbol?>("CommonAttributeClass")
                .Returns(type)
                .Verifiable();
        }

        public void SetUpAttributeModuleName(string moduleName)
        {
            AttributeModule
                .Setup(m => m.Name)
                .Returns(moduleName)
                .Verifiable();

            AttributeType
                .Setup(t => t.ContainingModule)
                .Returns(AttributeModule.Object)
                .Verifiable();

            SetUpAttributeType(AttributeType.Object);
        }

        public void SetUpAttributeTypeArity(int arity)
        {
            AttributeType
                .Setup(t => t.Arity)
                .Returns(arity)
                .Verifiable();

            SetUpAttributeModuleName("Mixer.dll");
        }

        internal void SetUpAttributeTypeArguments(params ITypeSymbol[] args)
        {
            AttributeType
                .Setup(t => t.TypeArguments)
                .Returns(ImmutableArray.Create(args))
                .Verifiable();

            SetUpAttributeTypeArity(args.Length);
        }

        internal void SetUpAttributeArguments(params TypedConstant[] args)
        {
            AttributeData
                .Protected()
                .Setup<ImmutableArray<TypedConstant>>("CommonConstructorArguments")
                .Returns(ImmutableArray.Create(args))
                .Verifiable();

            SetUpAttributeTypeArity(0);
        }
    }
}
