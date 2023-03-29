// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

[assembly: Parallelizable(ParallelScope.Children)]
[assembly: SetCulture("en-CA")]

namespace Mixer;

[SetUpFixture]
public static class TestSuite
{
    [OneTimeSetUp]
    public static void SetUp()
    {
        Tool.SetZeroVersionForTesting();
    }
}
