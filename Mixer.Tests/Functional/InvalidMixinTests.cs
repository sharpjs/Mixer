// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class InvalidMixinTests
{
    [Test]
    public void English()
    {
        new FunctionalTestBuilder()
        .WithSourceAndTargetKinds(Class)
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            enum InvalidMixin { }

            [Include<InvalidMixin>]
            partial $target Target { }
            """
        )
        .ExpectDiagnostic(
            "(5,2): error CS0592: " +
                "Attribute 'Mixin' is not valid on this declaration type. " +
                "It is only valid on 'class, struct' declarations."
        )
        .ExpectDiagnostic(
            "(8,2): error MIX0001: " +
                "Cannot include 'Test.InvalidMixin' because it is not a mixin. " +
                "Mixins must be marked with [Mixin]."
        )
        .Test();
    }

    [Test, SetUICulture("es-MX")]
    public void Spanish()
    {
        new FunctionalTestBuilder()
        .WithSourceAndTargetKinds(Class)
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            enum InvalidMixin { }

            [Include<InvalidMixin>]
            partial $target Target { }
            """
        )
        .ExpectDiagnostic(
            "(5,2): error CS0592: " +
                "El atributo 'Mixin' no es válido en este tipo de declaración. " +
                "Solo es válido en declaraciones 'clase, estructura'."
        )
        .ExpectDiagnostic(
            "(8,2): error MIX0001: " +
                "No se puede incluir 'Test.InvalidMixin' porque no es una mixin. " +
                "Los mixins deben estar marcados con [Mixin]."
        )
        .Test();
    }
}
