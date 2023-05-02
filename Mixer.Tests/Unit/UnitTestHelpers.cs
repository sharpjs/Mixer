// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Mixer.Tests.Unit.UnitTestHelpers;

using System.Reflection;

namespace Mixer.Tests.Unit;

internal static class UnitTestHelpers
{
    internal static TypedConstant MakeTypedConstant(TypedConstantKind kind, object? value)
    {
        var constant = default(TypedConstant);

        KindField .SetValueDirect(__makeref(constant), kind);
        ValueField.SetValueDirect(__makeref(constant), value!);

        return constant;
    }

    private const BindingFlags
        InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;

    private static readonly FieldInfo
        KindField  = typeof(TypedConstant).GetField("_kind",  InstanceNonPublic)!,
        ValueField = typeof(TypedConstant).GetField("_value", InstanceNonPublic)!;

}
