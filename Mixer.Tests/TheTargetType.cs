// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Text;
using Mixer;

namespace Test;

[Include(typeof(TheMixin<string>))]
[Include<TheMixin<StringBuilder>>, Include<TheMixin<FileInfo>>]
//[Include<int>] // will fail
internal partial class TheTargetType
{
}
