// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class NotAMixinTests
{
    [Test]
    public void English2()
    {
        new FunctionalTestBuilder().WithInput(
            """
            using Mixer;

            class NotAMixin { }

            [Include<NotAMixin>]
            partial class Target { }
            """
        )
        .ExpectDiagnostic(
            "(5,2): error MIX0001: " +
                "Cannot include 'NotAMixin' because it is not a mixin. " +
                "Mixins must be marked with [Mixin]."
        )
        .Test();
    }

    [Test, SetUICulture("es-MX")]
    public void Spanish()
    {
        new FunctionalTestBuilder().WithInput(
            """
            using Mixer;

            class NotAMixin { }

            [Include<NotAMixin>]
            partial class Target { }
            """
        )
        .ExpectDiagnostic(
            "(5,2): error MIX0001: " +
                "No se puede incluir 'NotAMixin' porque no es una mixin. " +
                "Los mixins deben estar marcados con [Mixin]."
        )
        .Test();
    }
}
