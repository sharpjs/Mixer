// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

// To cover: MixinGeneralizer.VisitIdentifierName

[TestFixture]
public class IdentifierNameTests
{
    [Test]
    public void NamedType()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    _ = typeof(Guid);
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
                void DoIt()
                {
                    _ = typeof(global::System.Guid);
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void ArrayType()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    _ = typeof(Guid[]);
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
                void DoIt()
                {
                    _ = typeof(global::System.Guid[]);
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }


    [Test]
    [TestCaseSource(typeof(Globals), nameof(PredefinedTypes))]
    public void PredefinedType(string keyword, string name)
    {
        new FunctionalTestBuilder()
        .WithSourceAndTargetKinds(Class) // to reduce duplication
        .WithInput(
            $$"""
            using Mixer;
            using System;

            namespace Test;

            [Mixin]
            $source Source
            {
                {{name}}? DoIt() => default;
            }

            [Include<Source>]
            partial $target Target { }
            """
        )
        .IgnoreUnexpectedDiagnostics()
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
                [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Mixer.Generator", "0.0.0.0")]
                {{keyword}}? DoIt() => default;
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Namespace()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    _ = typeof(System.Guid);
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
                void DoIt()
                {
                    _ = typeof(global::System.Guid);
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void InstanceMethod()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    "".GetHashCode();
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
                void DoIt()
                {
                    "".GetHashCode();
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void StaticMethod()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using static System.Guid;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    Parse("");
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
                void DoIt()
                {
                    global::System.Guid.Parse("");
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void ExtensionMethod()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            static class Extensions
            {
                public static bool IsNullOrEmpty(this string s)
                    => string.IsNullOrEmpty(s);
            }

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    "".IsNullOrEmpty();
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
                void DoIt()
                {
                    global::Test.Extensions.IsNullOrEmpty("");
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void StaticField()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using static Test.Stuff;

            namespace Test;

            static class Stuff
            {
                internal static object? Field = new();
            }

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    _ = Field;
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
                void DoIt()
                {
                    _ = global::Test.Stuff.Field;
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void StaticProperty()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using static Test.Stuff;

            namespace Test;

            static class Stuff
            {
                internal static object? Property { get; set; }
            }

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    _ = Property;
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
                void DoIt()
                {
                    _ = global::Test.Stuff.Property;
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void StaticEvent()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;
            using System;
            using static Test.Stuff;

            #pragma warning disable CS0067 // event never used

            namespace Test;

            static class Stuff
            {
                internal static event EventHandler? Event;
            }

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    Event += (_, _) => { };
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
                void DoIt()
                {
                    global::Test.Stuff.Event += (_, _) => { };
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Constructor()
    {
        new FunctionalTestBuilder()
        .WithSourceAndTargetKinds(Class) // to reduce duplication
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    _ = new Guid();
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
                void DoIt()
                {
                    _ = new global::System.Guid();
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void ConstructorOfPredefinedType()
    {
        new FunctionalTestBuilder()
        .WithSourceAndTargetKinds(Class) // to reduce duplication
        .WithInput(
            """
            using Mixer;
            using System;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    _ = new String("");
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
                void DoIt()
                {
                    _ = new string("");
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Var()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            #pragma warning disable CS0219 // variable assigned but never used

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    var x = 42;
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
                void DoIt()
                {
                    var x = 42;
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Unknown()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                void DoIt()
                {
                    Unknown();
                }
            }

            [Include<Source>]
            partial $target Target { }
            """
        )
        .ExpectDiagnostic(
            "(10,9): error CS0103: " +
            "The name 'Unknown' does not exist in the current context"
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
                void DoIt()
                {
                    Unknown();
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
