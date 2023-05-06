// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Mixer.Tests.Globals;

namespace Mixer.Tests;

internal static class Globals
{
    public static IEnumerable<TestCaseData> PredefinedTypes = new TestCaseData[]
    {
        new("object",  "Object" ),
        new("bool",    "Boolean"),
        new("char",    "Char"   ),
        new("sbyte",   "SByte"  ),
        new("byte",    "Byte"   ),
        new("short",   "Int16"  ),
        new("ushort",  "UInt16" ),
        new("int",     "Int32"  ),
        new("uint",    "UInt32" ),
        new("long",    "Int64"  ),
        new("ulong",   "UInt64" ),
        new("decimal", "Decimal"),
        new("float",   "Single" ),
        new("double",  "Double" ),
        new("string",  "String" )
    };
}
