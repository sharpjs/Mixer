// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class BaselineTests
{
    [Test]
    public void CSharp7()
    {
        new FunctionalTestBuilder()
        .WithLanguageVersion(LanguageVersion.CSharp7)
        .WithNullableOptions(NullableContextOptions.Disable)
        .WithSourceKinds(Class | Struct)
        .WithTargetKinds(Class | Struct)
        .WithInput(
            """
            using Mixer;

            namespace Test
            {
                [Mixin]
                $source Source
                {
                }

                [Include(typeof(Source))]
                partial $target Target
                {
                }
            }
            """
        )
        .ExpectGeneratedSource(
            "Test.Target.0.g.cs",
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test
            {
                #region Source

                partial $target Target
                {
                }

                #endregion
            }
            """
        )
        .Test();
    }

    [Test]
    public void CSharp8()
    {
        // C#8 = C#7 + nullable analysis

        new FunctionalTestBuilder()
        .WithLanguageVersion(LanguageVersion.CSharp8)
        .WithSourceKinds(Class | Struct)
        .WithTargetKinds(Class | Struct)
        .WithInput(
            """
            using Mixer;

            namespace Test
            {
                [Mixin]
                $source Source
                {
                }

                [Include(typeof(Source))]
                partial $target Target
                {
                }
            }
            """
        )
        .ExpectGeneratedSource(
            "Test.Target.0.g.cs",
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test
            {
                #region Source
                #nullable enable

                partial $target Target
                {
                }

                #nullable restore
                #endregion
            }
            """
        )
        .Test();
    }

    [Test]
    public void CSharp10()
    {
        // C#10 = C#8 + file-scoped namespaces

        new FunctionalTestBuilder()
        .WithLanguageVersion(LanguageVersion.CSharp10)
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source { }

            [Include(typeof(Source))]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
            "Test.Target.0.g.cs",
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
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void CSharp11()
    {
        // C#11 = C#10 + generic attributes

        new FunctionalTestBuilder()
        .WithLanguageVersion(LanguageVersion.CSharp11)
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source { }

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
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
