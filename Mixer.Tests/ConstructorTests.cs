// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class ConstructorTests
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
                public Source(int aNumber, Thing? aThing)
                    : base(/* ... */)
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
                public Target(int aNumber, global::Test.Thing? aThing)
                    : base(/* ... */)
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
