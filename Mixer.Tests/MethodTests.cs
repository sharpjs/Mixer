// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class MethodTests
{
    [Test]
    public void Simple()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;
            #pragma warning disable CS0067 // The event '...' is never used

            namespace Test;

            class Thing { }

            [Mixin]
            class Source
            {
                public void DoIt(int aNumber, Thing? aThing = default)
                {
                    // implementation
                }
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
                public void DoIt(int aNumber, global::Test.Thing? aThing = default)
                {
                    // implementation
                }
            }

            #nullable restore
            #endregion

            """
        });
    }
}
