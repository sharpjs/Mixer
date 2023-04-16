// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class InvalidMixinTests
{
    [Test]
    public void English()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            enum InvalidMixin { }
            """,
            """
            using Mixer;

            namespace Test;

            [Include<InvalidMixin>]
            partial class Target { }
            """
        );

        result.ShouldBeDiagnostics(
            "(5,2): error CS0592: " +
                "Attribute 'Mixin' is not valid on this declaration type. " +
                "It is only valid on 'class, struct' declarations."
        );
    }

    [Test, SetUICulture("es-MX")]
    public void Spanish()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            enum InvalidMixin { }
            """,
            """
            using Mixer;

            namespace Test;

            [Include<InvalidMixin>]
            partial class Target { }
            """
        );

        result.ShouldBeDiagnostics(
            "(5,2): error CS0592: " +
                "El atributo 'Mixin' no es válido en este tipo de declaración. " +
                "Solo es válido en declaraciones 'clase, estructura'."
        );
    }
}
