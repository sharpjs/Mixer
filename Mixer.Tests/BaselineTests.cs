// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class BaselineTests
{
    [Test]
    public void CSharp7()
    {
        var result = RunMixinGenerator(LanguageVersion.CSharp7,
            """
            using Mixer;

            namespace Test
            {
                [Mixin]
                class Source
                {
                }
            }
            """,
            """
            using Mixer;

            namespace Test
            {
                [Include(typeof(Source))]
                partial class Target
                {
                }
            }
            """
        );

        result.ShouldBeGeneratedSources(new()
        {
            // TODO: Improve newlines and indentation
            ["Test.Target.0.g.cs"] =
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test
            {
                #region Source

                partial class Target
                {
                }

                #endregion
            }

            """
        });
    }

    [Test]
    public void CSharp8()
    {
        // C#8 = C#7 + nullable analysis

        var result = RunMixinGenerator(LanguageVersion.CSharp8,
            """
            using Mixer;

            namespace Test
            {
                [Mixin]
                class Source
                {
                }
            }
            """,
            """
            using Mixer;

            namespace Test
            {
                [Include(typeof(Source))]
                partial class Target
                {
                }
            }
            """
        );

        result.ShouldBeGeneratedSources(new()
        {
            // TODO: Improve newlines and indentation
            ["Test.Target.0.g.cs"] =
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test
            {
                #region Source
                #nullable enable

                partial class Target
                {
                }

                #nullable restore
                #endregion
            }

            """
        });
    }

    [Test]
    public void CSharp10()
    {
        // C#10 = C#8 + file-scoped namespaces

        var result = RunMixinGenerator(LanguageVersion.CSharp10,
            """
            using Mixer;

            namespace Test;

            [Mixin]
            class Source { }
            """,
            """
            using Mixer;

            namespace Test;

            [Include(typeof(Source))]
            partial class Target { }
            """
        );

        result.ShouldBeGeneratedSources(new()
        {
            ["Test.Target.0.g.cs"] =
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
            }

            #nullable restore
            #endregion

            """
        });
    }

    [Test]
    public void CSharp11()
    {
        // C#11 = C#10 + generic attributes

        var result = RunMixinGenerator(LanguageVersion.CSharp11,
            """
            using Mixer;

            namespace Test;

            [Mixin]
            class Source { }
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
            }

            #nullable restore
            #endregion

            """
        });
    }
}
