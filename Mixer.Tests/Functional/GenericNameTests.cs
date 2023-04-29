// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

// To cover: MixinGeneralizer.VisitGenericName

[TestFixture]
public class GenericNameTests
{
    [Test]
    public void Simple()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source
            {
                public Func<string?, Guid>? Action { get; set; }
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
                public global::System.Func<string?, global::System.Guid>? Action { get; set; }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Qualified()
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
                public System.Func<string?, System.Guid>? Action { get; set; }
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
                public global::System.Func<string?, global::System.Guid>? Action { get; set; }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void QualifiedWithExternAlias()
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
                public global::System.Func<string?, global::System.Guid>? Action { get; set; }
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
                public global::System.Func<string?, global::System.Guid>? Action { get; set; }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void QualifiedWithUsingAlias()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using S = System;

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source
            {
                public S.Func<string?, S.Guid>? Action { get; set; }
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
                public global::System.Func<string?, global::System.Guid>? Action { get; set; }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void MixinType()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source<T0, T1>
            {
                public void Foo(Source<T0, T1>? other) { }
            }

            [Include<Source<int, Guid>>]
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
                public void Foo(Target? other) { }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void UnknownType()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source<T0, T1>
            {
                public UnknownType<int, Guid> Property { get; }
            }

            [Include<Source<int, Guid>>]
            partial $target Target { }
            """
        )
        .ExpectDiagnostic(
            "(11,12): error CS0246: " +
            "The type or namespace name 'UnknownType<,>' could not be found " +
            "(are you missing a using directive or an assembly reference?)"
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
                public UnknownType<int, global::System.Guid> Property { get; }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}