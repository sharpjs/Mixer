// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

[assembly: Parallelizable(ParallelScope.Children)]
[assembly: SetCulture  ("en-CA")] // for number and time formatting
[assembly: SetUICulture("en-CA")] // for resource files

namespace Mixer.Tests;

[SetUpFixture]
public static class TestSuite
{
    [OneTimeSetUp]
    public static void SetUp()
    {
        Tool.SetZeroVersionForTesting();
    }
}
