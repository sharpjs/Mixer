// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Mixer;

[Mixin]
internal class TheMixin<T> : IEquatable<T> where T : class
{
    bool IEquatable<T>.Equals(T? other) => false;
}
