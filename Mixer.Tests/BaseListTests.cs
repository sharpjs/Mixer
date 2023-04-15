// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class BaseListTests
{
    [Test]
    public void Simple()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;

            namespace Test;

            interface IFoo { }
            interface IBar { }

            [Mixin]
            class Source : IFoo, IBar { }
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
                : global::Test.IFoo, global::Test.IBar
            {
            }

            #nullable restore
            #endregion

            """
        });
    }
}