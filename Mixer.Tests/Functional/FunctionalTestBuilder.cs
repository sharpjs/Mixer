// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Reflection;

namespace Mixer.Tests.Functional;

/// <summary>
///   A builder to set up and run a functional test.
/// </summary>
internal partial class FunctionalTestBuilder
{
    private const string
        SourceKindPlaceholder = "$source",
        TargetKindPlaceholder = "$target";

    private readonly List<string>                     _inputs;
    private readonly List<string>                     _expectedDiagnostics;
    private readonly List<(string Name, string Text)> _expectedGeneratedSources;

    private LanguageVersion        _languageVersion;
    private NullableContextOptions _nullableOptions;
    private string[]               _sourceAndTargetKinds;

    /// <summary>
    ///   Initializes a new <see cref="FunctionalTestBuilder"/> instance using
    ///   C# 11 with nullable analysis enabled.
    /// </summary>
    public FunctionalTestBuilder()
    {
        _inputs                   = new();
        _expectedDiagnostics      = new();
        _expectedGeneratedSources = new();
        _languageVersion          = LanguageVersion.CSharp11;
        _nullableOptions          = NullableContextOptions.Enable;
        _sourceAndTargetKinds     = All;
    }

    /// <summary>
    ///   Sets the language version to use for the test.
    /// </summary>
    /// <param name="version">
    ///   The language version to use.
    /// </param>
    /// <returns>
    ///   The builder, to permit method chaining.
    /// </returns>
    public FunctionalTestBuilder WithLanguageVersion(LanguageVersion version)
    {
        _languageVersion = version;
        return this;
    }

    /// <summary>
    ///   Sets the nullable analysis options to use for the test.
    /// </summary>
    /// <param name="options">
    ///   The nullable analysis options to use.
    /// </param>
    /// <returns>
    ///   The builder, to permit method chaining.
    /// </returns>
    public FunctionalTestBuilder WithNullableOptions(NullableContextOptions options)
    {
        _nullableOptions = options;
        return this;
    }

    /// <summary>
    ///   Sets the kinds of source and target types to test.
    /// </summary>
    /// <param name="kinds">
    ///   The kinds of source and target types to test.
    /// </param>
    /// <returns>
    ///   The builder, to permit method chaining.
    /// </returns>
    public FunctionalTestBuilder WithSourceAndTargetKinds(params string[] kinds)
    {
        _sourceAndTargetKinds = kinds;
        return this;
    }

    /// <summary>
    ///   Adds a source to compile during the test.
    /// </summary>
    /// <param name="text">
    ///   The text of the source to compile.
    /// </param>
    /// <returns>
    ///   The builder, to permit method chaining.
    /// </returns>
    public FunctionalTestBuilder WithInput(string text)
    {
        _inputs.Add(text + Environment.NewLine);
        return this;
    }

    /// <summary>
    ///   Expects that the test will report the specified diagnostic.
    /// </summary>
    /// <param name="text">
    ///   The text of the diagnostic to expect.
    /// </param>
    /// <returns>
    ///   The builder, to permit method chaining.
    /// </returns>
    public FunctionalTestBuilder ExpectDiagnostic(string text)
    {
        _expectedDiagnostics.Add(text);
        return this;
    }

    /// <summary>
    ///   Expects that the test will generate the specified source.
    /// </summary>
    /// <param name="name">
    ///   The name of the generated source to expect.
    /// </param>
    /// <param name="text">
    ///   The text of the generated source to expect.
    /// </param>
    /// <returns>
    ///   The builder, to permit method chaining.
    /// </returns>
    public FunctionalTestBuilder ExpectGeneratedSource(string name, string text)
    {
        _expectedGeneratedSources.Add((name, text + Environment.NewLine));
        return this;
    }

    /// <summary>
    ///   Runs the test.
    /// </summary>
    public void Test()
    {
        foreach (var targetKind in _sourceAndTargetKinds)
        foreach (var sourceKind in _sourceAndTargetKinds)
            Test(sourceKind, targetKind);
    }

    private void Test(string sourceKind, string targetKind)
    {
        var syntaxTrees = Parse(sourceKind, targetKind);
        var compilation = Compile(syntaxTrees);

        if (compilation.GetDiagnostics() is { Length: > 0 } diagnostics)
        {
            Assert(diagnostics);
        }
        else
        {
            var result = RunMixinGenerator(compilation);

            result.Exception.Should().BeNull();
            Assert(result.Diagnostics);
            Assert(result.GeneratedSources, targetKind);
        }
    }

    private IEnumerable<SyntaxTree> Parse(string sourceKind, string targetKind)
    {
        var options = new CSharpParseOptions(_languageVersion);

        return _inputs.Select(source => Parse(source, options, sourceKind, targetKind));
    }

    private static SyntaxTree Parse(
        string             source,
        CSharpParseOptions options,
        string             sourceKind,
        string             targetKind)
    {
        source = source.Replace(SourceKindPlaceholder, sourceKind);
        source = source.Replace(TargetKindPlaceholder, targetKind);

        return CSharpSyntaxTree.ParseText(source, options);
    }

    private CSharpCompilation Compile(IEnumerable<SyntaxTree> syntaxTrees)
    {
        var name = TestContext.CurrentContext.Test.FullName;

        var options = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            nullableContextOptions: _nullableOptions
        );

        return CSharpCompilation.Create(name, syntaxTrees, MetadataReferences, options);
    }

    private GeneratorRunResult RunMixinGenerator(CSharpCompilation compilation)
    {
        return CSharpGeneratorDriver
            .Create(new MixinGenerator().AsSourceGenerator())
            .WithUpdatedParseOptions(new CSharpParseOptions(_languageVersion))
            .RunGenerators(compilation)
            .GetRunResult()
            .Results
            .Single();
    }

    private void Assert(ImmutableArray<Diagnostic> diagnostics_)
    {
        var diagnostics = diagnostics_
            .Select(d => d.ToString());

        diagnostics.Should().BeEquivalentTo(_expectedDiagnostics);
    }

    private void Assert(ImmutableArray<GeneratedSourceResult> generatedSources_, string targetKind)
    {
        var generatedSources = generatedSources_
            .ToDictionary(s => s.HintName, s => s.SourceText.ToString());

        var expected = _expectedGeneratedSources
            .ToDictionary(s => s.Name, s => s.Text.Replace(TargetKindPlaceholder, targetKind));

        generatedSources.Should().BeEquivalentTo(expected);
    }

    #region Metadata References

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

    #endregion
}
