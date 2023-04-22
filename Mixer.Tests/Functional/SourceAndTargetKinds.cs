// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Mixer.Tests.Functional.SourceAndTargetKinds;

namespace Mixer.Tests.Functional;

internal static class SourceAndTargetKinds
{
    public const string
               Class =         "class",
         RecordClass =  "record class",
              Struct =        "struct",
        RecordStruct = "record struct";

    public static readonly string[] All =
    {
               Class,
         RecordClass,
              Struct,
        RecordStruct,
    };
}
