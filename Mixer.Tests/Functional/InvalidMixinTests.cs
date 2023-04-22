// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class InvalidMixinTests
{
    [Test]
    public void English()
    {
        new FunctionalTestBuilder().WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            enum InvalidMixin { }

            [Include<InvalidMixin>]
            partial class Target { }
            """
        )
        .ExpectDiagnostic(
            "(5,2): error CS0592: " +
                "Attribute 'Mixin' is not valid on this declaration type. " +
                "It is only valid on 'class, struct' declarations."
        )
        .Test();
    }

    [Test, SetUICulture("es-MX")]
    public void Spanish()
    {
        new FunctionalTestBuilder().WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            enum InvalidMixin { }

            [Include<InvalidMixin>]
            partial class Target { }
            """
        )
        .ExpectDiagnostic(
            "(5,2): error CS0592: " +
                "El atributo 'Mixin' no es válido en este tipo de declaración. " +
                "Solo es válido en declaraciones 'clase, estructura'."
        )
        .Test();
    }
}
