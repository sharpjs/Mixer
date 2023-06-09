// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class OutputNameTests
{
    [Test]
    public void NestedNamespaceTest()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test.Nested;

            [Mixin]
            $source Source { }

            [Include<Source>]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
            "Test.Nested.Target.1.g.cs",
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test.Nested;

            #region Source
            #nullable enable

            partial $target Target
            {
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void ErrorTest()
    {
        new FunctionalTestBuilder()
        .WithSourceAndTargetKinds(Struct)
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source { }

            [Include<Source>]
            partial $target int { }
            """
        )
        .IgnoreUnexpectedDiagnostics()
        .ExpectGeneratedSource(
            "Test.ERROR.1.g.cs",
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial $target 
            {
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
