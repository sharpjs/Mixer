// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class AttributeTests
{
    [Test]
    public void Simple()
    {
        var result = RunMixinGenerator(
            """
            using System;
            using Mixer;

            namespace Test;

            class FooAttribute : Attribute { }

            [Mixin]
            [Foo]
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

            [global::Test.FooAttribute]
            partial class Target
            {
            }

            #nullable restore
            #endregion

            """
        });
    }

    [Test]
    public void SharedAttributeList()
    {
        var result = RunMixinGenerator(
            """
            using System;
            using Mixer;

            namespace Test;

            class FooAttribute : Attribute { }

            [Mixin, Foo]
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

            [global::Test.FooAttribute]
            partial class Target
            {
            }

            #nullable restore
            #endregion

            """
        });
    }

    [Test]
    public void MultiLineAttribute()
    {
        var result = RunMixinGenerator(
            """
            using System;
            using Mixer;

            namespace Test;

            class FooAttribute : Attribute { }

            [Mixin]
            [Foo(
                // stuff
            )]
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

            [global::Test.FooAttribute(
                // stuff
            )]
            partial class Target
            {
            }

            #nullable restore
            #endregion

            """
        });
    }
}
