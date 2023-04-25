// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

// Mock.Of<T>(x => ...) setups are false positives for this warning
#pragma warning disable RS1024 // Symbols should be compared for equality

namespace Mixer.Tests.Unit;

[TestFixture]
public class MixinGeneralizerTests
{
    // GUIDANCE: Prefer functional tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void IsMixinMarker_NullSymbol()
    {
        MixinGeneralizer.IsMixinMarker(null).Should().BeFalse();
    }

    [Test]
    public void IsMixinMarker_Symbol_NotInNamespace()
    {
        var type = Mock.Of<ITypeSymbol>(t
            => t.Name             == "MixinAttribute"
            && t.ContainingSymbol == Mock.Of<ITypeSymbol>()
        );

        MixinGeneralizer.IsMixinMarker(type).Should().BeFalse();
    }

    [Test]
    public void IsMixinMarker_Symbol_InNamespace_NotInNamespace()
    {
        var type = Mock.Of<ITypeSymbol>(t
            => t.Name             == "MixinAttribute"
            && t.ContainingSymbol == Mock.Of<INamespaceSymbol>(n
                => n.Name             == "Mixer"
                && n.ContainingSymbol == Mock.Of<IModuleSymbol>()
            )
        );

        MixinGeneralizer.IsMixinMarker(type).Should().BeFalse();
    }
}
