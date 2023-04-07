# Mixer

[![Build](https://github.com/sharpjs/Mixer/workflows/Build/badge.svg)](https://github.com/sharpjs/Mixer/actions)
[![NuGet](https://img.shields.io/nuget/v/Mixer.svg)](https://www.nuget.org/packages/Mixer)
[![NuGet](https://img.shields.io/nuget/dt/Mixer.svg)](https://www.nuget.org/packages/Mixer)

[Mixin](https://en.wikipedia.org/wiki/Mixin) support for C#.
When you can't inherit, include!

Mixer provides a smart `[Include]` attribute that copies the content of one
type into another type.  This enables you to 'inherit' from additional types
beyond the single base class that C# allows.

For example, this code:

```csharp
[Include<Pet>]
public partial class Dog : Mammal
{
    public void Bark() { ... }
}

[Mixin]
internal class Pet : IPet
{
    public string Name { get; set; }
}
```

compiles as if you had written:

```csharp
public class Dog : Mammal, IPet
{
    public void Bark() { ... }

    public string Name { get; set; }
}
```

Some facts about this example:

- `Dog` inherits the members of its base class, `Mammal`.
- An instance of `Dog` can be cast to `Mammal`.
- Through Mixer, `Dog` also _includes_ the members, interfaces, and attributes of `Pet`.
- An instance of `Dog` _cannot_ be cast to `Pet`.  C# allows only a single base class.
- An instance of `Dog` _can_ be cast to `IPet`.

Mixer is implemented as a [Roslyn incremental generator](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
for minimal impact to build times.  For each inclusion, Mixer generates a
partial type with the name of the target type and containing the members, base
types, and attributes of the mixin.

## Installation

Install [this NuGet Package](https://www.nuget.org/packages/Mixer) in your project.

## Usage

To make a class, structure, or record become includable (i.e. a *mixin*), add
the `[Mixin]` attribute to it.

```
using Mixer;

[Mixin]
internal class MyMixin { ... }
```

To include the mixin into some other class, structure, or record (i.e. a target
type), make the target type `partial` and add the `[Include]` attribute to it.
Mixer provides both generic and non-generic versions of this attribute.

```csharp
using Mixer;

// C# 11 and later:

[Include<MyMixin>]
partial class MyTargetType { ... }

// Any verison of C#:

[Include(typeof(MyMixin))]
partial class MyTargetType { ... }
```

That's it!

## Advanced Topics

### How do I see the generated code?

Add the following to the project file.

```xml
  <PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
  </PropertyGroup>
```

Builds will drop the generated files into the following folder:

`obj\{config}\{target}\generated\Mixer.Generator\Mixer.MixinGenerator\`

To see the generated files in Visual Studio's solution explorer:

```xml
  <ItemGroup Condition="'$(EmitCompilerGeneratedFiles)' == 'true'">
    <GeneratedFile Include="$(IntermediateOutputPath)generated\Mixer.Generator\Mixer.MixinGenerator\*.cs" />
    <None Include="@(GeneratedFile)" Link="Generated\%(Filename)%(Extension)" />
    <FileWrites Include="@(GeneratedFile)" />
  </ItemGroup>
```

<!--
  Copyright 2023 Subatomix Research Inc.
  SPDX-License-Identifier: ISC
-->
