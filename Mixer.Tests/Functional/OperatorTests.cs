// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class OperatorTests
{
    [Test]
    public void Simple()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using System;
            using Mixer;
            #pragma warning disable CS0067 // The event '...' is never used

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source
            {
                public static Source operator +(Source a, Source b)
                {
                    throw new NotImplementedException();
                }
            }

            [Include<Source>]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
            // TODO: Fix trivia in name replacement
            "Test.Target.1.g.cs",
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial $target Target
            {
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public static Targetoperator +(Targeta, Targetb)
                {
                    throw new global::System.NotImplementedException();
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Conversion()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using System;
            using Mixer;
            #pragma warning disable CS0067 // The event '...' is never used

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source
            {
                public static implicit operator string(Source a)
                {
                    throw new NotImplementedException();
                }
            }

            [Include<Source>]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
            // TODO: Fix trivia in name replacement
            "Test.Target.1.g.cs",
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial $target Target
            {
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public static implicit operator string(Targeta)
                {
                    throw new global::System.NotImplementedException();
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
