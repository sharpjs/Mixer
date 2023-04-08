// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

global using static Microsoft.CodeAnalysis.CSharp.LanguageVersion;
global using static Mixer.MixinGeneratorTestHelpers;

global using Nullable = Microsoft.CodeAnalysis.NullableContextOptions;
global using Sources  = System.Collections.Generic.Dictionary<string, string>;

using System.Collections.Immutable;
using System.Reflection;

namespace Mixer;

/// <summary>
///   Helper methods for testing <see cref="MixinGenerator"/>.
/// </summary>
internal static class MixinGeneratorTestHelpers
{
    /// <summary>
    ///   Runs <see cref="MixinGenerator"/> on a compilation of the specified
    ///   sources using C# 11 with nullable analysis enabled.
    /// </summary>
    /// <param name="sources">
    ///   The sources to compile.
    /// </param>
    /// <returns>
    ///   An object representing the result of the compilation and generator.
    /// </returns>
    internal static MixinGeneratorResult
        RunMixinGenerator(params string[] sources)
        => RunMixinGenerator(CSharp11, Nullable.Enable, sources);

    /// <summary>
    ///   Runs <see cref="MixinGenerator"/> on a compilation of the specified
    ///   sources using the specified C# language version with nullable
    ///   analysis enabled.
    /// </summary>
    /// <param name="version">
    ///   The C# language version to use.
    /// </param>
    /// <param name="sources">
    ///   The sources to compile.
    /// </param>
    /// <returns>
    ///   An object representing the result of the compilation and generator.
    /// </returns>
    internal static MixinGeneratorResult
        RunMixinGenerator(LanguageVersion version, params string[] sources)
        => RunMixinGenerator(version, Nullable.Enable, sources);

    /// <summary>
    ///   Runs <see cref="MixinGenerator"/> on a compilation of the specified
    ///   sources using C# 11 and the specified nullable analysis options.
    /// </summary>
    /// <param name="nullable">
    ///   The nullable analysis options to use.
    /// </param>
    /// <param name="sources">
    ///   The sources to compile.
    /// </param>
    /// <returns>
    ///   An object representing the result of the compilation and generator.
    /// </returns>
    internal static MixinGeneratorResult
        RunMixinGenerator(Nullable nullable, params string[] sources)
        => RunMixinGenerator(CSharp11, nullable, sources);

    /// <summary>
    ///   Runs <see cref="MixinGenerator"/> on a compilation of the specified
    ///   sources using the specified C# language version and nullable analysis
    ///   options.
    /// </summary>
    /// <param name="version">
    ///   The C# language version to use.
    /// </param>
    /// <param name="nullable">
    ///   The nullable analysis options to use.
    /// </param>
    /// <param name="sources">
    ///   The sources to compile.
    /// </param>
    /// <returns>
    ///   An object representing the result of the compilation and generator.
    /// </returns>
    internal static MixinGeneratorResult
        RunMixinGenerator(LanguageVersion version, Nullable nullable, params string[] sources)
    {
        var trees       = Parse(sources, version);
        var compilation = Compile(trees, nullable);

        if (compilation.GetDiagnostics() is { Length: > 0 } diagnostics)
            return new(diagnostics);

        var result = RunMixinGenerator(compilation);

        return new(result);
    }

    private static IEnumerable<SyntaxTree> Parse(string[] sources, LanguageVersion version)
    {
        var options = new CSharpParseOptions(version);

        return sources.Select(source => CSharpSyntaxTree.ParseText(source, options));
    }

    private static CSharpCompilation Compile(IEnumerable<SyntaxTree> trees, Nullable nullable)
    {
        var name = TestContext.CurrentContext.Test.FullName;

        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: nullable
        );

        return CSharpCompilation.Create(name, trees, MetadataReferences, options);
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
        var names = typeof(MixinAttribute).Assembly.GetReferencedAssemblies();
        var array = ImmutableArray.CreateBuilder<MetadataReference>(names.Length + 3);

        foreach (var name in names)
            array.Add(MakeReference(name));

        array.Add(MakeReference("System.Runtime"));
        array.Add(MakeReference(typeof(object)        .Assembly));
        array.Add(MakeReference(typeof(MixinAttribute).Assembly));

        return array.MoveToImmutable();
    }

    private static MetadataReference MakeReference(string assemblyName)
        => MakeReference(Assembly.Load(assemblyName));

    private static MetadataReference MakeReference(AssemblyName assemblyName)
        => MakeReference(Assembly.Load(assemblyName));

    private static MetadataReference MakeReference(Assembly assembly)
        => MetadataReference.CreateFromFile(assembly.Location);
}
