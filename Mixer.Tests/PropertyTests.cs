// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class PropertyTests
{
    [Test]
    public void Simple()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;

            namespace Test;

            class Thing { }

            [Mixin]
            class Source
            {
                public Thing? TheThing { get; set; }
            }
            """,
            """
            using Mixer;

            namespace Test;

            [Include<Source>]
            partial class Target { }
            """
        );

        result.ShouldBeGeneratedSources(new()
        {
            ["Test.Target.1.g.cs"] =
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial class Target
            {
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public global::Test.Thing? TheThing { get; set; }
            }

            #nullable restore
            #endregion

            """
        });
    }

    [Test]
    public void ExplicitInterfaceImplementation()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;

            namespace Test;

            class Thing { }
            interface IHasThing { Thing TheThing { get; } }

            [Mixin]
            class Source : IHasThing
            {
                Thing IHasThing.TheThing { get; } = new();
            }
            """,
            """
            using Mixer;

            namespace Test;

            [Include<Source>]
            partial class Target { }
            """
        );

        result.ShouldBeGeneratedSources(new()
        {
            ["Test.Target.1.g.cs"] =
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial class Target
                : global::Test.IHasThing
            {
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                global::Test.Thing global::Test.IHasThing.TheThing { get; } = new();
            }

            #nullable restore
            #endregion

            """
        });
    }
}
