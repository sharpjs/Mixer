// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class GenericMixinTests
{
    [Test]
    public void Simple()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            [Mixin]
            $source Source<T0, T1>
            {
                public T0? Property0 { get; set; }
                public T1? Property1 { get; set; }
            }

            [Include<Source<string, Guid>>]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
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
                public string? Property0 { get; set; }
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public global::System.Guid? Property1 { get; set; }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Nested()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            class Outer<T0>
            {
                [Mixin]
                public $source Source<T1>
                {
                    public T0? Property0 { get; set; }
                    public T1? Property1 { get; set; }
                }
            }

            [Include<Outer<string>.Source<Guid>>]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
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
                public string? Property0 { get; set; }
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public global::System.Guid? Property1 { get; set; }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
