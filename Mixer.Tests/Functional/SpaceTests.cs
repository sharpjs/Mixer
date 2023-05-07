// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

// To cover: SpaceNormalizer

[TestFixture]
public class SpaceTests
{
    [Test]
    public void NewlineReplacement()
    {
        new FunctionalTestBuilder()
        .WithInput(
            $$"""
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                public void Before() { } // comment{{        '\n'
          }}    public void Foo()    { } // comment{{'\r'}}{{'\n'
          }}    public void After()  { }
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
                public void Before() { } // comment
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public void Foo()    { } // comment
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public void After()  { }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void TabHandling_ZeroShift()
    {
        new FunctionalTestBuilder()
        .WithInput(
            $$"""
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
            {{'\t'}}public void Foo0() { }
             {{'\t'}}public void Foo1() { }
              {{'\t'}}public void Foo2() { }
               {{'\t'}}public void Foo3() { }
                {{'\t'}}public void Foo4() { }
            }

            [Include<Source>]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
            "Test.Target.1.g.cs",
            $$"""
            // <auto-generated>
            //   This code file was generated by the 'Mixer' NuGet package.
            //   See https://github.com/sharpjs/Mixer for more information.
            // </auto-generated>

            namespace Test;

            #region Source
            #nullable enable

            partial $target Target
            {
            {{'\t'}}[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
            {{'\t'}}public void Foo0() { }
             {{'\t'}}[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
             {{'\t'}}public void Foo1() { }
              {{'\t'}}[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
              {{'\t'}}public void Foo2() { }
               {{'\t'}}[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
               {{'\t'}}public void Foo3() { }
                {{'\t'}}[global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                {{'\t'}}public void Foo4() { }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void TabHandling_NonzeroShift()
    {
        new FunctionalTestBuilder()
        .WithInput(
            $$"""
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
            {{'\t'}}{{'\t'}}public void Foo0() { }
            {{'\t'}} {{'\t'}}public void Foo1() { }
            {{'\t'}}  {{'\t'}}public void Foo2() { }
            {{'\t'}}   {{'\t'}}public void Foo3() { }
            {{'\t'}}    {{'\t'}}public void Foo4() { }
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
                public void Foo0() { }
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public void Foo1() { }
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public void Foo2() { }
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public void Foo3() { }
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                    public void Foo4() { }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void StructuredTrivia()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
            #nullable enable
                public void Foo() { }
            #nullable restore
            }

            [Include<Source>]
            partial $target Target { }
            """
        )
        .ExpectGeneratedSource(
            // TODO: This is just a bug
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
                #nullable enable
                    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                public void Foo() { }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
