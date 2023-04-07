// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class GenericTargetTests
{
    [Test]
    public void SimpleGenericTarget()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;
            namespace Test;
            [Mixin] class Source { }
            """,
            """
            using Mixer;
            namespace Test;
            [Include<Source>]
            class Target<T0, T1> { }
            """
        );

        result.Diagnostics.Should().BeEmpty();

        result.GeneratedSources.Should().BeEquivalentTo(new Sources
        {
            ["Test.Target{T0,T1}.1.g.cs"] =
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial class Target<T0, T1>
            {
            }

            #nullable restore
            #endregion Source


            """
        });
    }

    [Test]
    public void GenericTargetWithNamedTypeConstraints()
    {
        var result = RunMixinGenerator(
            """
            using Mixer;
            namespace Test;
            [Mixin] class Source { }
            """,
            """
            using System;
            using Mixer;
            namespace Test;
            [Include<Source>]
            class Target<T0, T1>
                where T0 : Attribute, IFormattable, new()
                where T1 : notnull, new()
            { }
            """
        );

        result.Diagnostics.Should().BeEmpty();

        result.GeneratedSources.Should().BeEquivalentTo(new Sources
        {
            ["Test.Target{T0,T1}.1.g.cs"] =
            """
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial class Target<T0, T1>
                where T0 : global::System.Attribute, global::System.IFormattable, new()
                where T1 : notnull, new()
            {
            }

            #nullable restore
            #endregion Source


            """
        });
    }
}