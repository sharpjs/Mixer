// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Mixer.MixinGeneratorTestHelpers;

using System.Collections.Immutable;
using System.Reflection;

namespace Mixer;

internal static class MixinGeneratorTestHelpers
{
    internal static KeyValuePair<string, string> Source(string name, string text)
        => KeyValuePair.Create(name, text);

    internal static MixinGeneratorResult
        RunMixinGenerator(params string[] sources)
        => RunMixinGenerator(LanguageVersion.CSharp11, nullable: true, sources);

    internal static MixinGeneratorResult
        RunMixinGenerator(LanguageVersion version, params string[] sources)
        => RunMixinGenerator(version, nullable: true, sources);

    internal static MixinGeneratorResult
        RunMixinGenerator(bool nullable, params string[] sources)
        => RunMixinGenerator(LanguageVersion.CSharp11, nullable, sources);

    internal static MixinGeneratorResult
        RunMixinGenerator(LanguageVersion version, bool nullable, params string[] sources)
    {
        var compilation = Compile(sources, version, nullable);

        if (compilation.GetDiagnostics() is { Length: > 0 } diagnostics)
            return new(diagnostics);

        var result = RunMixinGenerator(compilation);

        return new(ref result);
    }

    private static CSharpCompilation Compile(
        IEnumerable<string> sources,
        LanguageVersion     version,
        bool                nullable)
    {
        var name = TestContext.CurrentContext.Test.FullName;

        var compilationOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: GetNullableOptions(nullable)
        );

        var parseOptions = new CSharpParseOptions(version);

        var trees = sources.Select(s => CSharpSyntaxTree.ParseText(s, parseOptions));

        return CSharpCompilation.Create(name, trees, MetadataReferences, compilationOptions);
    }

    private static NullableContextOptions GetNullableOptions(bool enable)
    {
        return enable
            ? NullableContextOptions.Enable
            : NullableContextOptions.Disable;
    }

    private static GeneratorRunResult RunMixinGenerator(CSharpCompilation compilation)
    {
        return CSharpGeneratorDriver
            .Create(new MixinGenerator().AsSourceGenerator())
            .RunGenerators(compilation)
            .GetRunResult()
            .Results
            .Single();
    }

    private static readonly ImmutableArray<MetadataReference>
        MetadataReferences = GetMetadataReferences();

    private static ImmutableArray<MetadataReference> GetMetadataReferences()
    {
        var array = ImmutableArray.CreateBuilder<MetadataReference>(initialCapacity: 4);

        foreach (var name in typeof(MixinAttribute).Assembly.GetReferencedAssemblies())
            array.Add(MakeReference(name));

        array.Add(MakeReference("System.Runtime"));
        array.Add(MakeReference(typeof(object)));
        array.Add(MakeReference(typeof(MixinAttribute)));

        return array.Count == array.Capacity
            ? array.MoveToImmutable()
            : array.ToImmutable();
    }

    private static MetadataReference MakeReference(string assemblyName)
    {
        return MakeReference(Assembly.Load(assemblyName));
    }

    private static MetadataReference MakeReference(AssemblyName assemblyName)
    {
        return MakeReference(Assembly.Load(assemblyName));
    }

    private static MetadataReference MakeReference(Type type)
    {
        return MakeReference(type.Assembly);
    }

    private static MetadataReference MakeReference(Assembly assembly)
    {
        return MetadataReference.CreateFromFile(assembly.Location);
    }
}
