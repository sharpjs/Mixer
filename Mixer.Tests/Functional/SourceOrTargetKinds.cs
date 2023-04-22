// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Mixer.Tests.Functional.SourceOrTargetKinds;

namespace Mixer.Tests.Functional;

[Flags]
internal enum SourceOrTargetKinds : byte
{
    Class        = 1 << 0,
    RecordClass  = 1 << 1,
    Struct       = 1 << 2,
    RecordStruct = 1 << 3,

    All = Class | RecordClass | Struct | RecordStruct,
}

internal static class SourceOrTargetKindsExtensions
{
    public static bool HasAny(this SourceOrTargetKinds value, SourceOrTargetKinds flags)
    {
        return (value & flags) != 0;
    }

    public static bool HasAny(this SourceOrTargetKinds value, int flags)
    {
        return (value & (SourceOrTargetKinds) flags) != 0;
    }
}
