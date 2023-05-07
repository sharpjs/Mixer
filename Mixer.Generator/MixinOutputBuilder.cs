// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Microsoft.CodeAnalysis.Text;

namespace Mixer;

using K = SyntaxKind;

/// <summary>
///   Generates the code to include a set of mixins into a target.
/// </summary>
internal ref struct MixinOutputBuilder
{
    private readonly INamedTypeSymbol      _targetType;
    private readonly ImmutableArray<Mixin> _mixins;
    private readonly LanguageVersion       _version;
    private readonly SyntaxTrivia          _endOfLine;
    private readonly CancellationToken     _cancellation;

    private int _indent;

    /// <summary>
    ///   Initializes a new <see cref="MixinOutputBuilder"/> instance.
    /// </summary>
    public MixinOutputBuilder(
        INamedTypeSymbol      targetType,
        ImmutableArray<Mixin> mixins,
        LanguageVersion       languageVersion,
        CancellationToken     cancellation = default)
    {
        Debug.Assert(mixins.Any());

        _targetType   = targetType;
        _mixins       = mixins;
        _version      = languageVersion;
        _endOfLine    = GetPlatformEndOfLine();
        _cancellation = cancellation;
    }

    /// <summary>
    ///   Generates the code to include the mixins into the target.
    /// </summary>
    public SourceText Build()
    {
        return MakeCompilationUnit().GetText(Encoding.UTF8);
    }

    private CompilationUnitSyntax MakeCompilationUnit()
    {
        return CompilationUnit(
            externs:        default,
            usings:         default,
            attributeLists: default,
            members:        List(MakeNamespace())
        );
    }

    private ImmutableArray<MemberDeclarationSyntax> MakeNamespace()
    {
        var scope = _targetType.ContainingNamespace;

        if (scope.IsGlobalNamespace)
            return RenderMixinsInGlobalNamespace();

        var name = Qualify(scope, alias: null);

        return ImmutableArray.Create(
            _version.SupportsFileScopedNamespaces()
                ? MakeFileScopedNamespace (name)
                : MakeBlockScopedNamespace(name)
        );
    }

    private ImmutableArray<MemberDeclarationSyntax> RenderMixinsInGlobalNamespace()
    {
        return RenderMixins().AddLeadingTrivia(MakePreamble());
    }

    private MemberDeclarationSyntax MakeFileScopedNamespace(NameSyntax name)
    {
        return FileScopedNamespaceDeclaration(
            attributeLists:   default,
            modifiers:        default,
            namespaceKeyword: Token(MakePreamble(), K.NamespaceKeyword, TriviaList(Space)),
            name:             name,
            semicolonToken:   Token(default, K.SemicolonToken, TriviaList(_endOfLine, _endOfLine)),
            externs:          default,
            usings:           default,
            members:          List(RenderMixins())
        );
    }

    private MemberDeclarationSyntax MakeBlockScopedNamespace(NameSyntax name)
    {
        var oldIndent = Indent();
        var content   = RenderMixins();
        Restore(oldIndent);

        return NamespaceDeclaration(
            attributeLists:   default,
            modifiers:        default,
            namespaceKeyword: Token(MakePreamble(), K.NamespaceKeyword, TriviaList(Space)),
            name:             name.WithTrailingTrivia(_endOfLine),
            openBraceToken:   Token(default, K.OpenBraceToken, TriviaList(_endOfLine)),
            externs:          default,
            usings:           default,
            members:          List(content),
            closeBraceToken:  Token(default, K.CloseBraceToken, TriviaList(_endOfLine)),
            semicolonToken:   default
        );
    }

    private SyntaxTriviaList MakePreamble()
    {
        return TriviaList(
            Comment("// <auto-generated>"),                                             _endOfLine,
            Comment("//   This code file was generated by the 'Mixer' NuGet package."), _endOfLine,
            Comment("//   See https://github.com/sharpjs/Mixer for more information."), _endOfLine,
            Comment("// </auto-generated>"),                                            _endOfLine,
            _endOfLine
        );
    }

    private ImmutableArray<MemberDeclarationSyntax> RenderMixins()
    {
        var array = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>(_mixins.Length);

        foreach (var mixin in _mixins)
            array.Add(RenderMixin(mixin));

        return array.MoveToImmutable();
    }

    private MemberDeclarationSyntax RenderMixin(Mixin mixin)
    {
        var oldIndents = IndentForNesting();
        var rendered   = Nest(oldIndents, RenderMixinCore(mixin));

        return _version.SupportsNullableAnalysis()
            ? WrapInTriviaWithNullable   (mixin, rendered)
            : WrapInTriviaWithoutNullable(mixin, rendered);
    }

    private int[] IndentForNesting()
    {
        var count = 0;

        for (var type = _targetType; type.ContainingType is { } container; type = container)
            count++;

        if (count == 0)
            return Array.Empty<int>();

        var indents = new int[count];

        for (var i = 0; i < indents.Length; i++)
            indents[i] = Indent();

        return indents;
    }

    private TypeDeclarationSyntax Nest(int[] oldIndents, TypeDeclarationSyntax declaration)
    {
        var type = _targetType;

        for (var i = oldIndents.Length - 1; i >= 0; i--)
        {
            Restore(oldIndents[i]);
            declaration = MakeType(type = type.ContainingType!, declaration);
        }

        return declaration;
    }

    private TypeDeclarationSyntax MakeType(INamedTypeSymbol type, MemberDeclarationSyntax member)
    {
        return type switch
        {
            { IsRecord:    true } => MakeRecord(type, member),
            { IsValueType: true } => MakeStruct(type, member),
            _                     => MakeClass (type, member),
        };
    }

    private TypeDeclarationSyntax MakeClass(INamedTypeSymbol type, MemberDeclarationSyntax member)
    {
        return ClassDeclaration(
            attributeLists:    default,
            modifiers:         TokenList(Token(IndentationList(), K.PartialKeyword, OneSpace())),
            keyword:           TokenAndSpace(K.ClassKeyword),
            identifier:        RenderIdentifier(type),
            typeParameterList: RenderTypeParameterList(type),
            baseList:          default,
            constraintClauses: RenderConstraintClauses(type),
            openBraceToken:    Token(IndentationList(), K.OpenBraceToken, OneEndOfLine()),
            members:           SingletonList(member),
            closeBraceToken:   Token(IndentationList(), K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:    default
        );
    }

    private TypeDeclarationSyntax MakeStruct(INamedTypeSymbol type, MemberDeclarationSyntax member)
    {
        return StructDeclaration(
            attributeLists:    default,
            modifiers:         TokenList(Token(IndentationList(), K.PartialKeyword, OneSpace())),
            keyword:           TokenAndSpace(K.StructKeyword),
            identifier:        RenderIdentifier(type),
            typeParameterList: RenderTypeParameterList(type),
            baseList:          default,
            constraintClauses: RenderConstraintClauses(type),
            openBraceToken:    Token(IndentationList(), K.OpenBraceToken, OneEndOfLine()),
            members:           SingletonList(member),
            closeBraceToken:   Token(IndentationList(), K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:    default
        );
    }

    private TypeDeclarationSyntax MakeRecord(INamedTypeSymbol type, MemberDeclarationSyntax member)
    {
        var (declarationKind, keywordKind) = type.IsReferenceType
            ? (K.RecordDeclaration,       K.ClassKeyword)
            : (K.RecordStructDeclaration, K.StructKeyword);

        return RecordDeclaration(
            kind:                 declarationKind,
            attributeLists:       default,
            modifiers:            TokenList(Token(IndentationList(), K.PartialKeyword, OneSpace())),
            keyword:              TokenAndSpace(K.RecordKeyword),
            classOrStructKeyword: TokenAndSpace(keywordKind),
            identifier:           RenderIdentifier(type),
            typeParameterList:    RenderTypeParameterList(type),
            parameterList:        default,
            baseList:             default,
            constraintClauses:    RenderConstraintClauses(type),
            openBraceToken:       Token(IndentationList(), K.OpenBraceToken, OneEndOfLine()),
            members:              SingletonList(member),
            closeBraceToken:      Token(IndentationList(), K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:       default
        );
    }

    private TypeDeclarationSyntax RenderMixinCore(Mixin mixin)
    {
        var content = Specialize(mixin);

        return _targetType switch
        {
            { IsRecord:    true } => RenderMixinToRecord(content),
            { IsValueType: true } => RenderMixinToStruct(content),
            _                     => RenderMixinToClass (content),
        };
    }

    private TypeDeclarationSyntax Specialize(Mixin mixin)
    {
        var content = mixin.GetContent(_cancellation);

        return new MixinSpecializer(mixin.Type, _targetType).Specialize(content);
    }

    private TypeDeclarationSyntax RenderMixinToClass(TypeDeclarationSyntax content)
    {
        return ClassDeclaration(
            attributeLists:    Normalize(content.AttributeLists),
            modifiers:         TokenList(Token(IndentationList(), K.PartialKeyword, OneSpace())),
            keyword:           TokenAndSpace(K.ClassKeyword),
            identifier:        RenderIdentifier(_targetType),
            typeParameterList: RenderTypeParameterList(_targetType),
            baseList:          Normalize(content.BaseList),
            constraintClauses: RenderConstraintClauses(_targetType),
            openBraceToken:    Token(IndentationList(), K.OpenBraceToken, OneEndOfLine()),
            members:           Normalize(content.Members),
            closeBraceToken:   Token(IndentationList(), K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:    default
        );
    }

    private TypeDeclarationSyntax RenderMixinToStruct(TypeDeclarationSyntax content)
    {
        return StructDeclaration(
            attributeLists:    Normalize(content.AttributeLists),
            modifiers:         TokenList(Token(IndentationList(), K.PartialKeyword, OneSpace())),
            keyword:           TokenAndSpace(K.StructKeyword),
            identifier:        RenderIdentifier(_targetType),
            typeParameterList: RenderTypeParameterList(_targetType),
            baseList:          Normalize(content.BaseList),
            constraintClauses: RenderConstraintClauses(_targetType),
            openBraceToken:    Token(IndentationList(), K.OpenBraceToken, OneEndOfLine()),
            members:           Normalize(content.Members),
            closeBraceToken:   Token(IndentationList(), K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:    default
        );
    }

    private TypeDeclarationSyntax RenderMixinToRecord(TypeDeclarationSyntax content)
    {
        var (declarationKind, keywordKind) = _targetType.IsReferenceType
            ? (K.RecordDeclaration,       K.ClassKeyword)
            : (K.RecordStructDeclaration, K.StructKeyword);

        return RecordDeclaration(
            kind:                 declarationKind,
            attributeLists:       Normalize(content.AttributeLists),
            modifiers:            TokenList(Token(IndentationList(), K.PartialKeyword, OneSpace())),
            keyword:              TokenAndSpace(K.RecordKeyword),
            classOrStructKeyword: TokenAndSpace(keywordKind),
            identifier:           RenderIdentifier(_targetType),
            typeParameterList:    RenderTypeParameterList(_targetType),
            parameterList:        default,
            baseList:             Normalize(content.BaseList),
            constraintClauses:    RenderConstraintClauses(_targetType),
            openBraceToken:       Token(IndentationList(), K.OpenBraceToken, OneEndOfLine()),
            members:              Normalize(content.Members),
            closeBraceToken:      Token(IndentationList(), K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:       default
        );
    }

    private BaseListSyntax? Normalize(BaseListSyntax? baseList)
    {
        return baseList is null
            ? null
            : new SpaceNormalizer().Normalize(baseList, _indent + IndentSize);
    }

    private SyntaxList<AttributeListSyntax> Normalize(SyntaxList<AttributeListSyntax> attributeLists)
    {
        return new SpaceNormalizer().Normalize(attributeLists, _indent);
    }

    private SyntaxList<MemberDeclarationSyntax> Normalize(SyntaxList<MemberDeclarationSyntax> members)
    {
        return new SpaceNormalizer().Normalize(members, _indent + IndentSize);
    }

    private SyntaxToken RenderIdentifier(INamedTypeSymbol type)
    {
        var trailingTrivia = type.Arity != 0 ? default : OneEndOfLine();

        return Identifier(default, type.Name, trailingTrivia);
    }

    private TypeParameterListSyntax? RenderTypeParameterList(INamedTypeSymbol type)
    {
        if (type.Arity == 0)
            return default;

        return TypeParameterList(
            lessThanToken:    Token(K.LessThanToken),
            parameters:       MakeCommaSeparatedList(type.TypeParameters, RenderTypeParameter),
            greaterThanToken: Token(default, K.GreaterThanToken, OneEndOfLine())
        );
    }

    private static TypeParameterSyntax RenderTypeParameter(ITypeParameterSymbol parameter)
    {
        return TypeParameter(
            attributeLists:  default,
            varianceKeyword: RenderVariance(parameter.Variance),
            Identifier(parameter.Name)
        );
    }

    private static SyntaxToken RenderVariance(VarianceKind variance)
    {
        return variance switch
        {
            VarianceKind.In  => TokenAndSpace(K.InKeyword),
            VarianceKind.Out => TokenAndSpace(K.OutKeyword),
            _                => default
        };
    }

    private SyntaxList<TypeParameterConstraintClauseSyntax> RenderConstraintClauses(INamedTypeSymbol type)
    {
        if (type.Arity == 0)
            return default;

        var count = 0;

        foreach (var parameter in type.TypeParameters)
            if (parameter.HasConstraints())
                count++;

        if (count == 0)
            return default;

        var array = ImmutableArray.CreateBuilder<TypeParameterConstraintClauseSyntax>(count);

        foreach (var parameter in type.TypeParameters)
            if (parameter.HasConstraints())
                array.Add(RenderConstraintClause(parameter));

        return List(array.MoveToImmutable());
    }

    private TypeParameterConstraintClauseSyntax RenderConstraintClause(ITypeParameterSymbol parameter)
    {
        return TypeParameterConstraintClause(
            whereKeyword: Token( TriviaList(Whitespace("    ")), K.WhereKeyword, OneSpace() ),
            name:         IdentifierName( Identifier(default, parameter.Name, OneSpace()) ),
            colonToken:   Token(default, K.ColonToken, OneSpace()),
            constraints:  RenderConstraints(parameter)
        )
        .WithTrailingTrivia(_endOfLine);
    }

    private SeparatedSyntaxList<TypeParameterConstraintSyntax> RenderConstraints(ITypeParameterSymbol parameter)
    {
        /*
            [] = optional; () = conditional separator

            type_parameter_constraints
                : [primary_constraint] (',') [secondary_constraints] (',') [constructor_constraint]

            primary_constraint
                : class_type | 'class' | 'struct' | 'notnull' | 'unmanaged'

            secondary_constraints
                : [secondary_constraints] (',') interface_type
                | [secondary_constraints] (',') type_parameter

            constructor_constraint
                : 'new' '(' ')'
        */

        var types = parameter.ConstraintTypes;

        // Count a non-special primary and any secondary constraints
        var count = types.Length;

        // Count a special primary constraint
        if (parameter.HasSpecialPrimaryConstraint())
            count++;

        // Count a constructor constraint
        if (parameter.HasConstructorConstraint)
            count++;

        var array = ImmutableArray.CreateBuilder<SyntaxNodeOrToken>(count * 2 - 1);

        // Special primary constraint
        if (parameter.HasUnmanagedTypeConstraint)
            array.Add(UnmanagedConstraint());
        else if (parameter.HasReferenceTypeConstraint)
            array.Add(ClassOrStructConstraint(K.ClassConstraint));
        else if (parameter.HasValueTypeConstraint)
            array.Add(ClassOrStructConstraint(K.StructConstraint));
        else if (parameter.HasNotNullConstraint)
            array.Add(NotNullConstraint());

        // Non-special primary and any secondary constraints
        foreach (var type in types)
            array.AddCommaSeparated(TypeConstraint(Qualify(type)));
            // NOTE: No predefined type is usable as a constraint.

        // Constructor constraint
        if (parameter.HasConstructorConstraint)
            array.AddCommaSeparated(ConstructorConstraint());

        return SeparatedList<TypeParameterConstraintSyntax>(array.MoveToImmutable());
    }

    private MemberDeclarationSyntax WrapInTriviaWithoutNullable(
        Mixin mixin, TypeDeclarationSyntax rendered)
    {
        return rendered
            .WithLeadingTrivia(
                TriviaList(
                    MakeBeginRegionDirective(mixin),
                    _endOfLine
                )
                .AddRange(rendered.GetLeadingTrivia())
            )
            .WithTrailingTrivia(
                _endOfLine,
                _endOfLine,
                MakeEndRegionDirective()
            );
    }

    private MemberDeclarationSyntax WrapInTriviaWithNullable(
        Mixin mixin, TypeDeclarationSyntax rendered)
    {
        return rendered
            .WithLeadingTrivia(
                TriviaList(
                    MakeBeginRegionDirective(mixin),
                    MakeNullableDirective(K.EnableKeyword),
                    _endOfLine
                )
                .AddRange(rendered.GetLeadingTrivia())
            )
            .WithTrailingTrivia(
                _endOfLine,
                _endOfLine,
                MakeNullableDirective(K.RestoreKeyword),
                MakeEndRegionDirective()
            );
    }

    private SyntaxTrivia MakeBeginRegionDirective(Mixin mixin)
    {
        return Trivia(RegionDirectiveTrivia(
            Token(IndentationList(), K.HashToken, default),
            TokenAndSpace(K.RegionKeyword),
            Token(
                TriviaList(PreprocessingMessage(mixin.Type.Name)),
                K.EndOfDirectiveToken,
                OneEndOfLine()
            ),
            isActive: true
        ));
    }

    private SyntaxTrivia MakeEndRegionDirective()
    {
        return Trivia(EndRegionDirectiveTrivia(
            Token(IndentationList(), K.HashToken, default),
            Token(K.EndRegionKeyword),
            Token(default, K.EndOfDirectiveToken, OneEndOfLine()),
            isActive: true
        ));
    }

    private SyntaxTrivia MakeNullableDirective(K settingKind, SyntaxToken target = default)
    {
        return Trivia(NullableDirectiveTrivia(
            Token(IndentationList(), K.HashToken, default),
            TokenAndSpace(K.NullableKeyword),
            Token(settingKind),
            target,
            Token(default, K.EndOfDirectiveToken, OneEndOfLine()),
            isActive: true
        ));
    }

    private int Indent()
    {
        var oldIndent = _indent;
        _indent = oldIndent + IndentSize;
        return oldIndent;
    }

    private void Restore(int level)
        => _indent = level;

    private SyntaxTriviaList IndentationList()
        => _indent > 0 ? TriviaList(IndentationCore()) : default;

#if USEFUL_IN_FUTURE
    private SyntaxTrivia Indentation()
        => _indent > 0 ? IndentationCore() : default;
#endif

    private SyntaxTrivia IndentationCore()
        => Whitespace(new string(' ', _indent));

    private SyntaxTriviaList OneEndOfLine()
        => TriviaList(_endOfLine);
}
