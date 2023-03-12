// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics;

namespace Mixer;

/// <summary>
///   Copies the members of the specified type into the current type.
/// </summary>
/// <remarks>
///   ★ The specified type must be marked with <see cref="MixinAttribute"/>.
/// </remarks>
[AttributeUsage(Class | Struct, AllowMultiple = true, Inherited = false)]
[Conditional("MIXER_COMPILE_TIME_CONTENT")]
public class IncludeAttribute : Attribute
{
    /// <summary>
    ///   Copies the members of the specified type into the current type.
    /// </summary>
    /// <param name="type">
    ///   <para>
    ///     The type from which to copy members.
    ///   </para>
    ///   <para>
    ///     ★ The type must be marked with <see cref="MixinAttribute"/>.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///   <paramref name="type"/> is <see langword="null"/>.
    /// </exception>
    /// <remarks>
    ///   ★ The specified type must be marked with <see cref="MixinAttribute"/>.
    /// </remarks>
    public IncludeAttribute(Type type)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    /// <param name="type">
    ///   Gets the type from which to copy members.
    /// </param>
    public Type Type { get; }
}

/// <summary>
///   Copies the members of the specified type into the current type.
/// </summary>
/// <typeparam name="T">
///   <para>
///     The type from which to copy members.
///   </para>
///   <para>
///     ★ The type must be marked with <see cref="MixinAttribute"/>.
///   </para>
/// </typeparam>
/// <remarks>
///   ★ The specified type must be marked with <see cref="MixinAttribute"/>.
/// </remarks>
[AttributeUsage(Class | Struct, AllowMultiple = true, Inherited = false)]
[Conditional("MIXER_COMPILE_TIME_CONTENT")]
public class IncludeAttribute<T> : IncludeAttribute
{
    /// <summary>
    ///   Copies the members of the specified type into the current type.
    /// </summary>
    /// <remarks>
    ///   ★ The specified type must be marked with <see cref="MixinAttribute"/>.
    /// </remarks>
    public IncludeAttribute()
        : base(typeof(T)) { }
}
