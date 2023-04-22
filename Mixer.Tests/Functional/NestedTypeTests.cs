// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class NestedTypeTests
{
    [Test]
    public void Class()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                internal class ThingDoer
                {
                    public void DoTheThing() { }

                    private class Helper { }
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
                internal class ThingDoer
                {
                    public void DoTheThing() { }

                    private class Helper { }
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Struct()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                private readonly ref struct ThingDoer
                {
                    public void DoTheThing() { }

                    private struct Helper { }
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
                private readonly ref struct ThingDoer
                {
                    public void DoTheThing() { }

                    private struct Helper { }
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Interface()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                public interface IThingDoer
                {
                    void DoTheThing();

                    public interface IHelper { }
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
                public interface IThingDoer
                {
                    void DoTheThing();

                    public interface IHelper { }
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void RecordClass()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                private record class Thing(string? Name)
                //             ^^^^^ optional
                {
                    public bool IsNamed => Name is { Length: > 0 };

                    private record class InnerThing(string? InnerName);
                    //             ^^^^^ optional
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
                private record class Thing(string? Name)
                //             ^^^^^ optional
                {
                    public bool IsNamed => Name is { Length: > 0 };

                    private record class InnerThing(string? InnerName);
                    //             ^^^^^ optional
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void RecordStruct()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                private readonly record struct Thing(string? Name)
                {
                    public bool IsNamed => Name is { Length: > 0 };

                    private readonly record struct InnerThing(string? InnerName);
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
                private readonly record struct Thing(string? Name)
                {
                    public bool IsNamed => Name is { Length: > 0 };

                    private readonly record struct InnerThing(string? InnerName);
                }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Delegate()
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
                public delegate Thing ThingAction(Thing aThing);
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
                public delegate global::Test.Thing ThingAction(global::Test.Thing aThing);
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }

    [Test]
    public void Enum()
    {
        new FunctionalTestBuilder()
        .WithInput(
            """
            using Mixer;

            namespace Test;

            [Mixin]
            $source Source
            {
                internal enum Stooges { Larry, Curly, Moe }
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
                internal enum Stooges { Larry, Curly, Moe }
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
