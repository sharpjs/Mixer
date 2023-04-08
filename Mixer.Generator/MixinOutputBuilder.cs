// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using Microsoft.CodeAnalysis.Text;

namespace Mixer;

using K = SyntaxKind;

/// <summary>
///   Generates the code to include a set of mixins into a target.
/// </summary>
internal readonly ref struct MixinOutputBuilder
{
    private readonly INamedTypeSymbol      _targetType;
    private readonly ImmutableArray<Mixin> _mixins;
    private readonly LanguageVersion       _version;
    private readonly SyntaxTrivia          _endOfLine;
    private readonly CancellationToken     _cancellation;

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
        return
            MakeCompilationUnit(
                MakeNamespace(
                    RenderMixins()
                )
            )
            .GetText(Encoding.UTF8);
    }

    private CompilationUnitSyntax MakeCompilationUnit(
        ImmutableArray<MemberDeclarationSyntax> members)
    {
        return CompilationUnit(
            externs:        default,
            usings:         default,
            attributeLists: default,
            members:        List(members)
        );
    }

    private ImmutableArray<MemberDeclarationSyntax> MakeNamespace(
        ImmutableArray<MemberDeclarationSyntax> members)
    {
        var scope = _targetType.ContainingNamespace;

        if (scope.IsGlobalNamespace)
            return members.SetItem(0, AddPreamble(members[0]));

        var name = Qualify(scope, alias: null);

        return ImmutableArray.Create(
            _version.SupportsFileScopedNamespaces()
                ? MakeFileScopedNamespace (name, members)
                : MakeBlockScopedNamespace(name, members)
        );
    }

    private MemberDeclarationSyntax AddPreamble(MemberDeclarationSyntax member)
    {
        return member.WithLeadingTrivia(
            MakePreambleTrivia().AddRange(member.GetLeadingTrivia())
        );
    }

    private SyntaxTriviaList MakePreambleTrivia()
    {
        return TriviaList(
            Comment("// <auto-generated>"),                                             _endOfLine,
            Comment("//   This code file was generated by the 'Mixer' NuGet package."), _endOfLine,
            Comment("//   See https://github.com/sharpjs/Mixer for more information."), _endOfLine,
            Comment("// </auto-generated>"),                                            _endOfLine,
            _endOfLine
        );
    }

    private MemberDeclarationSyntax MakeBlockScopedNamespace(
        NameSyntax                              name,
        ImmutableArray<MemberDeclarationSyntax> members)
    {
        return NamespaceDeclaration(
            attributeLists:   default,
            modifiers:        default,
            namespaceKeyword: Token(MakePreambleTrivia(), K.NamespaceKeyword, TriviaList(Space)),
            name:             name.WithTrailingTrivia(_endOfLine),
            openBraceToken:   Token(default, K.OpenBraceToken, TriviaList(_endOfLine)),
            externs:          default,
            usings:           default,
            members:          List(members),
            closeBraceToken:  Token(default, K.CloseBraceToken, TriviaList(_endOfLine)),
            semicolonToken:   default
        );
    }

    private MemberDeclarationSyntax MakeFileScopedNamespace(
        NameSyntax                              name,
        ImmutableArray<MemberDeclarationSyntax> members)
    {
        return FileScopedNamespaceDeclaration(
            attributeLists:   default,
            modifiers:        default,
            namespaceKeyword: Token(MakePreambleTrivia(), K.NamespaceKeyword, TriviaList(Space)),
            name:             name,
            semicolonToken:   Token(default, K.SemicolonToken, TriviaList(_endOfLine, _endOfLine)),
            externs:          default,
            usings:           default,
            members:          List(members)
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
        var content = Specialize(mixin);

        var rendered = _targetType switch
        {
            { IsRecord:    true } => RenderMixinToRecord(content),
            { IsValueType: true } => RenderMixinToStruct(content),
            _                     => RenderMixinToClass (content),
        };

        return _version.SupportsNullableAnalysis()
            ? WrapInTriviaWithNullable   (mixin, rendered)
            : WrapInTriviaWithoutNullable(mixin, rendered);
    }

    private TypeDeclarationSyntax Specialize(Mixin mixin)
    {
        var content = mixin.GetContent(_cancellation);

        return new MixinSpecializer(mixin.Type, _targetType).Specialize(content);
    }

    private TypeDeclarationSyntax RenderMixinToClass(TypeDeclarationSyntax content)
    {
        return ClassDeclaration(
            attributeLists:    content.AttributeLists,
            modifiers:         TokenList(TokenAndSpace(K.PartialKeyword)),
            keyword:           TokenAndSpace(K.ClassKeyword),
            identifier:        RenderTargetIdentifier(),
            typeParameterList: RenderTypeParameterList(),
            baseList:          content.BaseList,
            constraintClauses: RenderConstraintClauses(),
            openBraceToken:    Token(default, K.OpenBraceToken, OneEndOfLine()),
            members:           content.Members,
            closeBraceToken:   Token(default, K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:    default
        );
    }

    private TypeDeclarationSyntax RenderMixinToStruct(TypeDeclarationSyntax content)
    {
        return StructDeclaration(
            attributeLists:    content.AttributeLists,
            modifiers:         TokenList(TokenAndSpace(K.PartialKeyword)),
            keyword:           TokenAndSpace(K.StructKeyword),
            identifier:        RenderTargetIdentifier(),
            typeParameterList: RenderTypeParameterList(),
            baseList:          content.BaseList,
            constraintClauses: RenderConstraintClauses(),
            openBraceToken:    Token(default, K.OpenBraceToken, OneEndOfLine()),
            members:           content.Members,
            closeBraceToken:   Token(default, K.CloseBraceToken, OneEndOfLine()),
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
            attributeLists:       content.AttributeLists,
            modifiers:            TokenList(TokenAndSpace(K.PartialKeyword)),
            keyword:              TokenAndSpace(K.RecordKeyword),
            classOrStructKeyword: TokenAndSpace(keywordKind),
            identifier:           RenderTargetIdentifier(),
            typeParameterList:    RenderTypeParameterList(),
            parameterList:        default,
            baseList:             content.BaseList,
            constraintClauses:    RenderConstraintClauses(),
            openBraceToken:       Token(default, K.OpenBraceToken, OneEndOfLine()),
            members:              content.Members,
            closeBraceToken:      Token(default, K.CloseBraceToken, OneEndOfLine()),
            semicolonToken:       default
        );
    }

    private SyntaxToken RenderTargetIdentifier()
    {
        return Identifier(
            default,
            _targetType.Name,
            _targetType.Arity == 0 ? OneEndOfLine() : default
        );
    }

    private TypeParameterListSyntax? RenderTypeParameterList()
    {
        if (_targetType.Arity == 0)
            return default;

        return TypeParameterList(
            lessThanToken:    Token(K.LessThanToken),
            parameters:       MakeCommaSeparatedList(_targetType.TypeParameters, RenderTypeParameter),
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

    private SyntaxList<TypeParameterConstraintClauseSyntax> RenderConstraintClauses()
    {
        if (_targetType.Arity == 0)
            return default;

        var count = 0;

        foreach (var parameter in _targetType.TypeParameters)
            if (parameter.HasConstraints())
                count++;

        if (count == 0)
            return default;

        var array = ImmutableArray.CreateBuilder<TypeParameterConstraintClauseSyntax>(count);

        foreach (var parameter in _targetType.TypeParameters)
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
        if (parameter.HasReferenceTypeConstraint)
            array.Add(ClassOrStructConstraint(K.ClassKeyword));
        else if (parameter.HasValueTypeConstraint)
            array.Add(ClassOrStructConstraint(K.StructKeyword));
        else if (parameter.HasNotNullConstraint)
            array.Add(NotNullConstraint());
        else if (parameter.HasUnmanagedTypeConstraint)
            array.Add(UnmanagedConstraint());

        // Non-special primary and any secondary constraints
        foreach (var type in types)
            array.AddCommaSeparated(TypeConstraint(Qualify(type)));

        // Constructor constraint
        if (parameter.HasConstructorConstraint)
            array.AddCommaSeparated(ConstructorConstraint());

        return SeparatedList<TypeParameterConstraintSyntax>(array.MoveToImmutable());
    }

    private static TypeParameterConstraintSyntax RenderConstraint(ITypeSymbol constraint)
    {
        return ClassOrStructConstraint(K.ClassConstraint);
    }

    private MemberDeclarationSyntax WrapInTriviaWithoutNullable(
        Mixin mixin, TypeDeclarationSyntax rendered)
    {
        return rendered
            .WithLeadingTrivia(
                MakeBeginRegionDirective(mixin),
                _endOfLine
            )
            .WithTrailingTrivia(
                _endOfLine,
                _endOfLine,
                MakeEndRegionDirective(mixin)
            );
    }

    private MemberDeclarationSyntax WrapInTriviaWithNullable(
        Mixin mixin, TypeDeclarationSyntax rendered)
    {
        return rendered
            .WithLeadingTrivia(
                MakeBeginRegionDirective(mixin),
                MakeNullableDirective(K.EnableKeyword),
                _endOfLine
            )
            .WithTrailingTrivia(
                _endOfLine,
                _endOfLine,
                MakeNullableDirective(K.RestoreKeyword),
                MakeEndRegionDirective(mixin)
            );
    }

    private SyntaxTrivia MakeBeginRegionDirective(Mixin mixin)
    {
        return Trivia(RegionDirectiveTrivia(
            Token(K.HashToken),
            TokenAndSpace(K.RegionKeyword),
            Token(
                TriviaList(PreprocessingMessage(mixin.Type.Name)),
                K.EndOfDirectiveToken,
                OneEndOfLine()
            ),
            isActive: true
        ));
    }

    private SyntaxTrivia MakeEndRegionDirective(Mixin mixin)
    {
        return Trivia(EndRegionDirectiveTrivia(
            Token(K.HashToken),
            TokenAndSpace(K.EndRegionKeyword),
            Token(
                TriviaList(PreprocessingMessage(mixin.Type.Name)),
                K.EndOfDirectiveToken,
                OneEndOfLine()
            ),
            isActive: true
        ));
    }

    private SyntaxTrivia MakeNullableDirective(K settingKind, SyntaxToken target = default)
    {
        return Trivia(NullableDirectiveTrivia(
            Token(K.HashToken),
            TokenAndSpace(K.NullableKeyword),
            Token(settingKind),
            target,
            Token(default, K.EndOfDirectiveToken, OneEndOfLine()),
            isActive: true
        ));
    }

    private SyntaxTriviaList OneEndOfLine()
    {
        return TriviaList(_endOfLine);
    }
}
