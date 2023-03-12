// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics;

namespace Mixer;

/// <summary>
///   Declares a type as a mixin.
/// </summary>
/// <remarks>
///   A type marked with this attribute can be included into another partial
///   type by marking that type with <see cref="IncludeAttribute"/>.
/// </remarks>
[AttributeUsage(Class | Struct, AllowMultiple = false, Inherited = false)]
[Conditional("MIXER_COMPILE_TIME_CONTENT")]
public sealed class MixinAttribute : Attribute { }
