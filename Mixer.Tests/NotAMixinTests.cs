// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

[TestFixture]
public class NotAMixinTests
{
    [Test]
    public void NotAMixin()
    {
        var result = RunMixinGenerator(
            "class NotAMixin { }",
            "using Mixer; namespace Test; [Include<NotAMixin>] class Target { }"
        );

        result.ShouldBeDiagnostics(
            "(1,31): error MIX0001: " +
                "Cannot include 'NotAMixin' because it is not a mixin. " +
                "Mixins must be marked with [Mixin]."
        );
    }
}
