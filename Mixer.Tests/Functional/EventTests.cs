// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer.Tests.Functional;

[TestFixture]
public class EventTests
{
    [Test]
    public void Simple()
    {
        new FunctionalTestBuilder().WithInput(
            """
            using System;
            using Mixer;
            #pragma warning disable CS0067 // The event '...' is never used

            namespace Test;

            class Thing { }

            [Mixin]
            $source Source
            {
                public event EventHandler? ItHappened;
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
                public event global::System.EventHandler? ItHappened;
            }

            #nullable restore
            #endregion
            """
        )
        .Test();
    }
}
