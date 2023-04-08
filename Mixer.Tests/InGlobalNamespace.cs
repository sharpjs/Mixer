// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class InGlobalNamespace
{
    [Test]
    public void Test()
    {
        var result = RunMixinGenerator(
            """
            [Mixer.Mixin]
            class Source { }
            """,
            """
            [Mixer.Include(typeof(Source))]
            partial class Target { }
            """
        );

        result.ShouldBeGeneratedSources(new()
        {
            ["Target.0.g.cs"] =
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            #region Source
            #nullable enable

            partial class Target
            {
            }

            #nullable restore
            #endregion Source


            """
        });
    }
}
