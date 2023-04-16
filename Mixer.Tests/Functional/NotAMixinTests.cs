// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class NotAMixinTests
{
    [Test]
    public void English2()
    {
        var result = RunMixinGenerator(
            """
            class NotAMixin { }
            """,
            """
            using Mixer;

            namespace Test;

            [Include<NotAMixin>]
            partial class Target { }
            """
        );

        result.ShouldBeDiagnostics(
            "(5,2): error MIX0001: " +
                "Cannot include 'NotAMixin' because it is not a mixin. " +
                "Mixins must be marked with [Mixin]."
        );
    }

    [Test, SetUICulture("es-MX")]
    public void Spanish()
    {
        var result = RunMixinGenerator(
            """
            class NotAMixin { }
            """,
            """
            using Mixer;

            namespace Test;

            [Include<NotAMixin>]
            partial class Target { }
            """
        );

        result.ShouldBeDiagnostics(
            "(5,2): error MIX0001: " +
                "No se puede incluir 'NotAMixin' porque no es una mixin. " +
                "Los mixins deben estar marcados con [Mixin]."
        );
    }
}
