// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class ConstructorTests
{
    [Test]
    public void Simple()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source
            {
                public Source(int aNumber, Thing? aThing)
                {
                    // implementation
                }
            }

            [Include<Source>]
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
                public Target(int aNumber, global::Test.Thing? aThing)
                {
                    // implementation
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
