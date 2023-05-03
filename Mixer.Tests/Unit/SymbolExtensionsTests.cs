// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Text;

namespace Mixer.Tests.Unit;

[TestFixture]
public class SymbolExtensionsTests
{
    // GUIDANCE: Prefer functional tests.  Use unit tests to cover code paths
    // that are difficult or impossible to cover with integration tests.

    [Test]
    public void AppendForFileName_TypeSymbol_UnsupportedKind()
    {
        var ns = Mock.Of<INamespaceSymbol>();
        Mock.Get(ns).Setup(n => n.IsGlobalNamespace).Returns(true);

        var t1 = Mock.Of<ITypeSymbol>();
        Mock.Get(t1).Setup(t => t.Kind).Returns(SymbolKind.ErrorType);

        var t0 = Mock.Of<INamedTypeSymbol>();
        Mock.Get(t0).Setup(t => t.ContainingSymbol).Returns(ns);
        Mock.Get(t0).Setup(t => t.Name).Returns("T0");
        Mock.Get(t0).Setup(t => t.Arity).Returns(1);
        Mock.Get(t0).Setup(t => t.TypeArguments).Returns(ImmutableArray.Create(t1));

        new StringBuilder().AppendForFileName(t0).ToString()
            .Should().Be("T0{ERROR}");
    }
}
