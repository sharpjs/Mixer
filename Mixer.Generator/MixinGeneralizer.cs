// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

// references
// https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/grammar

namespace Mixer;

/// <summary>
///   A C# syntax rewriter that converts a mixin from its declaration form to a
///   generalized form suitable for inclusion into any targets.
/// </summary>
/// <remarks>
///   <list type="bullet">
///     <item>
///       Removes <c>MixinAttribute</c> from the mixin type declaration.
///     </item>
///     <item>
///       Removes modifiers, type parameters, record parameters, and constraint
///       clauses from the mixin type declaration.
///     </item>
///     <item>
///       Replaces references in the mixin body to the mixin type with a target
///       type placeholder <c>$This</c>.
///     </item>
///     <item>
///       Replaces type parameters in the mixin body with placeholders
///       <c>$T0</c>, <c>$T1</c>, and so on.
///     </item>
///     <item>
///       Fully qualifies type names.
///     </item>
///     <item>
///       Expands extension method invocations.
///     </item>
///     <item>
///       Applies <see cref="System.CodeDom.Compiler.GeneratedCodeAttribute"/>
///       to first-level child declarations in the mixin body.
///     </item>
///     <item>
///       Makes miscellaneous spacing changes for readability of the eventual
///       generated code.
///     </item>
///   </list>
/// </remarks>
internal class MixinGeneralizer : CSharpSyntaxRewriter
{
    private readonly INamedTypeSymbol                    _mixinType;
    private readonly SemanticModel                       _semanticModel;
    private readonly ImmutableDictionary<string, string> _placeholders;
    private readonly SyntaxTrivia                        _endOfLine;
    private readonly CancellationToken                   _cancellation;

    /// <summary>
    ///   Initializes a new <see cref="MixinGeneralizer"/> instance.
    /// </summary>
    public MixinGeneralizer(
        INamedTypeSymbol  mixinType,
        SemanticModel     semanticModel,
        CancellationToken cancellation)
    {
        _mixinType     = mixinType;
        _semanticModel = semanticModel;
        _placeholders  = GenerateTypeParameterPlaceholders(mixinType);
        _endOfLine     = GetPlatformEndOfLine();
        _cancellation  = cancellation;

        // TODO: Dictionary of module names to extern aliases, since Roslyn does not provide it.
    }

    /// <summary>
    ///   Converts a mixin from its declaration form to a generalized form
    ///   suitable for inclusion into any targets.
    /// </summary>
    public Mixin Generalize(TypeDeclarationSyntax declaration)
    {
        var nullableContext = _semanticModel.GetNullableContext(declaration.SpanStart);

        declaration = (TypeDeclarationSyntax) Visit(declaration)!;

        return new(_mixinType, declaration, nullableContext);
    }

    #region Visitor Methods: Class, Struct, and Record Declarations

    public override SyntaxNode? VisitClassDeclaration(ClassDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (ClassDeclarationSyntax) base.VisitClassDeclaration(node)!;

        node = _declarationDepth switch
        {
            1 => MixinDeclaration(node),
            2 => node.AddAttributeLists(MakeGeneratedCodeAttributeList(node)),
            _ => node,
        };

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitStructDeclaration(StructDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (StructDeclarationSyntax) base.VisitStructDeclaration(node)!;

        node = _declarationDepth switch
        {
            1 => MixinDeclaration(node),
            2 => node.AddAttributeLists(MakeGeneratedCodeAttributeList(node)),
            _ => node,
        };

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitRecordDeclaration(RecordDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (RecordDeclarationSyntax) base.VisitRecordDeclaration(node)!;

        node = _declarationDepth switch
        {
            1 => MixinDeclaration(node),
            2 => node.AddAttributeLists(MakeGeneratedCodeAttributeList(node)),
            _ => node,
        };

        LeaveDeclaration();
        return node;
    }

    // Mixin type declarations serve only as containers for attribute lists,
    // base lists, and members.

    private static ClassDeclarationSyntax MixinDeclaration(ClassDeclarationSyntax node)
    {
        return ClassDeclaration(
            node.AttributeLists, default,
            node.Identifier,     default,
            node.BaseList,       default,
            node.Members
        );
    }

    private static StructDeclarationSyntax MixinDeclaration(StructDeclarationSyntax node)
    {
        return StructDeclaration(
            node.AttributeLists, default,
            node.Identifier,     default,
            node.BaseList,       default,
            node.Members
        );
    }

    private static RecordDeclarationSyntax MixinDeclaration(RecordDeclarationSyntax node)
    {
        return RecordDeclaration(
            node.Kind(),
            node.AttributeLists, default,
            node.Keyword,
            node.Identifier,     default, default,
            node.BaseList,       default,
            node.Members
        );
    }

    #endregion
    #region Visitor Methods: Other Declarations and Attribute Targets

    public override SyntaxNode? VisitEnumDeclaration(EnumDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (EnumDeclarationSyntax) base.VisitEnumDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (InterfaceDeclarationSyntax) base.VisitInterfaceDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitDelegateDeclaration(DelegateDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (DelegateDeclarationSyntax) base.VisitDelegateDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (ConstructorDeclarationSyntax) base.VisitConstructorDeclaration(node)!;

        if (IsChildDeclaration)
            node = node
                .WithIdentifier(Identifier(TargetTypeNamePlaceholder))
                .AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitDestructorDeclaration(DestructorDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (DestructorDeclarationSyntax) base.VisitDestructorDeclaration(node)!;

        if (IsChildDeclaration)
            node = node
                .WithIdentifier(Identifier(TargetTypeNamePlaceholder))
                .AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitFieldDeclaration(FieldDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (FieldDeclarationSyntax) base.VisitFieldDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (PropertyDeclarationSyntax) base.VisitPropertyDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitAccessorDeclaration(AccessorDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (AccessorDeclarationSyntax) base.VisitAccessorDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitIndexerDeclaration(IndexerDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (IndexerDeclarationSyntax) base.VisitIndexerDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitEventDeclaration(EventDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (EventDeclarationSyntax) base.VisitEventDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitEventFieldDeclaration(EventFieldDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (EventFieldDeclarationSyntax) base.VisitEventFieldDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (MethodDeclarationSyntax) base.VisitMethodDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitOperatorDeclaration(OperatorDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (OperatorDeclarationSyntax) base.VisitOperatorDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    public override SyntaxNode? VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
    {
        EnterDeclaration();
        node = (ConversionOperatorDeclarationSyntax) base.VisitConversionOperatorDeclaration(node)!;

        if (IsChildDeclaration)
            node = node.AddAttributeLists(MakeGeneratedCodeAttributeList(node));

        LeaveDeclaration();
        return node;
    }

    #endregion
    #region Visitor Methods: Attribute and Base Lists

    public override SyntaxNode? VisitAttributeList(AttributeListSyntax node)
    {
        node = (AttributeListSyntax) base.VisitAttributeList(node)!;

        // Attribute list should disappear if all of its attributes disappear
        return node.Attributes.Any() ? node : null;
    }

    public override SyntaxNode? VisitAttribute(AttributeSyntax node)
    {
        return IsMixinDeclaration && IsMixinMarker(node)
            ? null // disappears
            : base.VisitAttribute(node);
    }

    public override SyntaxNode? VisitBaseList(BaseListSyntax node)
    {
        node = (BaseListSyntax) base.VisitBaseList(node)!;

        if (IsMixinDeclaration)
            node = node.WithTrailingTrivia(TriviaList(_endOfLine));

        return node;
    }

    #endregion
    #region Visitor Methods: Names

    // Type representation in Roslyn:
    //
    // Type:
    // - ArrayType            ─╮
    // - FunctionPointerType   │ 
    // - NullableType          │
    // - OmittedTypeArgument   ├ composite types;
    // - PointerType           │ qualify by visiting components
    // - PredefinedType        │
    // - RefType               │
    // - TupleTypeSyntax      ─╯
    // - Name:
    //   - AliasQualifiedName => do nothing (already fully qualified)
    //   - QualifiedName      => fully qualify LHS
    //   - SimpleName:
    //     - GenericName      => fully qualify LHS¹, visit type arguments
    //     - IdentifierName   => fully qualify¹
    //
    // ¹when not the RHS of a qualified name

    private bool _isRhsOfQualifiedName;

    public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
    {
        // Get symbol before node is changed by visitor
        var symbol = GetSymbol(node);

        // Replace mixin name with placeholder
        if (IsMixinType(symbol))
            return IdentifierName(TargetTypeNamePlaceholder);

        // Visit LHS and dot
        var left  = (NameSyntax?) Visit(node.Left) ?? throw new ArgumentNullException("left");
        var dot   = VisitToken(node.DotToken);

        // Visit RHS
        _isRhsOfQualifiedName = true;
        var right = (SimpleNameSyntax?) Visit(node.Right) ?? throw new ArgumentNullException("right");
        _isRhsOfQualifiedName = false;

        return node.Update(left, dot, right);
    }

    public override SyntaxNode? VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
    {
        // Get symbol before node is changed by visitor
        var symbol = GetSymbol(node);

        // Replace mixin name with placeholder
        if (IsMixinType(symbol))
            return IdentifierName(TargetTypeNamePlaceholder);

        // Visit LHS and colon
        var left  = (IdentifierNameSyntax?) Visit(node.Alias) ?? throw new ArgumentNullException("alias");
        var colon = VisitToken(node.ColonColonToken);

        // Visit RHS
        _isRhsOfQualifiedName = true;
        var right = (SimpleNameSyntax?) Visit(node.Name) ?? throw new ArgumentNullException("name");
        _isRhsOfQualifiedName = false;

        return node.Update(left, colon, right);
    }

    public override SyntaxNode? VisitTypeArgumentList(TypeArgumentListSyntax node)
    {
        var wasRightOfQualifiedName = _isRhsOfQualifiedName;
        _isRhsOfQualifiedName = false;

        var result = base.VisitTypeArgumentList(node);

        _isRhsOfQualifiedName = wasRightOfQualifiedName;
        return result;
    }

    public override SyntaxNode? VisitGenericName(GenericNameSyntax node)
    {
        // Get symbol before node is changed by visitor
        var symbol = GetSymbol(node);

        // Replace mixin name with placeholder
        if (IsMixinType(symbol))
            return IdentifierName(TargetTypeNamePlaceholder);

        node = (GenericNameSyntax) base.VisitGenericName(node)!;

        // Further qualify a qualified name only when in its LHS
        if (_isRhsOfQualifiedName)
            return node;

        if (symbol is null)
            return node;

        return Qualify(symbol, node);
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        // Get symbol before node is changed by visitor
        var symbol = GetSymbol(node);

        // Replace mixin name with placeholder
        if (IsMixinType(symbol))
            return IdentifierName(TargetTypeNamePlaceholder);

        node = (IdentifierNameSyntax) base.VisitIdentifierName(node)!;

        // Further qualify a qualified name only when in its LHS
        if (_isRhsOfQualifiedName)
            return node;

        if (node.IsVar)
            return node;

        if (symbol is null)
            return node;

        switch (symbol.Kind)
        {
            case SymbolKind.NamedType:
            case SymbolKind.Namespace when symbol is INamespaceSymbol { IsGlobalNamespace: false }:
            case SymbolKind.Method    when symbol is IMethodSymbol    { MethodKind: MethodKind.Ordinary, IsStatic: true }:
            case SymbolKind.Method    when symbol is IMethodSymbol    { MethodKind: MethodKind.ReducedExtension }:
            case SymbolKind.Field     when symbol is IFieldSymbol     { IsStatic: true }:
            case SymbolKind.Property  when symbol is IPropertySymbol  { IsStatic: true }:
            case SymbolKind.Event     when symbol is IEventSymbol     { IsStatic: true }:
                return Qualify(symbol, node);

            case SymbolKind.Method when symbol is IMethodSymbol { MethodKind: MethodKind.Constructor }:
                return Qualify(symbol.ContainingType);

            case SymbolKind.TypeParameter:
                return IdentifierName(_placeholders[symbol.Name]);

            default:
                return node;
        }
    }

    private bool IsMixinType(ISymbol? symbol)
    {
        return SymbolEqualityComparer.Default.Equals(symbol, _mixinType);
    }

    #endregion
    #region Visitor Methods: Invocations

    public override SyntaxNode? VisitInvocationExpression(InvocationExpressionSyntax node)
    {
        if (!IsExtensionMethodInvocation(node, out var expression))
            return base.VisitInvocationExpression(node);

        // Expand extension method invocation

        var name = (NameSyntax?) Visit(expression.Name)
            ?? throw new ArgumentNullException("name");

        var target = (ExpressionSyntax?) Visit(expression.Expression)
            ?? throw new ArgumentNullException("expression");

        var args = (ArgumentListSyntax?) Visit(node.ArgumentList)
            ?? throw new ArgumentNullException("argumentList");

        args = args.WithArguments(args.Arguments.Insert(0, Argument(target)));

        return node.Update(name, args);
    }

    #endregion

    #region Helpers: Declaration Depth Tracking

    private uint _declarationDepth;

    private void EnterDeclaration() => _declarationDepth++;
    private void LeaveDeclaration() => _declarationDepth--;
    private bool IsMixinDeclaration => _declarationDepth == 1;
    private bool IsChildDeclaration => _declarationDepth == 2;

    #endregion
    #region Helpers: Semantic Model Interaction

    private ISymbol? GetSymbol(ExpressionSyntax node)
        => _semanticModel.GetSymbolInfo(node, _cancellation).Symbol;

    private ITypeSymbol? GetType(AttributeSyntax node)
        => _semanticModel.GetTypeInfo(node, _cancellation).ConvertedType;

    private bool IsMixinMarker(AttributeSyntax node)
    {
        return GetType(node)         is                  { Name: "MixinAttribute"  } type
            && type.ContainingSymbol is INamespaceSymbol { Name: "Mixer"           } ns1
            && ns1 .ContainingSymbol is INamespaceSymbol { IsGlobalNamespace: true } ns0
            && ns0 .ContainingSymbol is IModuleSymbol    { Name: "Mixer.dll"       };
    }

    private bool IsExtensionMethodInvocation(
        InvocationExpressionSyntax                              node,
        [MaybeNullWhen(false)] out MemberAccessExpressionSyntax expression)
    {
        expression = node.Expression as MemberAccessExpressionSyntax;

        return expression is not null
            && GetSymbol(node) is IMethodSymbol { IsExtensionMethod: true };
    }

    #endregion
    #region Helpers: Type Parameter Placeholder Generation

    private static ImmutableDictionary<string, string>
        GenerateTypeParameterPlaceholders(INamedTypeSymbol type)
    {
        var dictionary = ImmutableDictionary.CreateBuilder<string, string>();

        GenerateTypeParameterPlaceholders(dictionary, type);

        return dictionary.ToImmutable();
    }

    private static void
        GenerateTypeParameterPlaceholders(
            ImmutableDictionary<string, string>.Builder dictionary,
            INamedTypeSymbol                            type
        )
    {
        if (type.ContainingType is { } parent)
            GenerateTypeParameterPlaceholders(dictionary, parent);

        foreach (var parameter in type.TypeParameters)
            dictionary.Add(parameter.Name, GetPlaceholder(dictionary.Count));
    }

    #endregion
    #region Helpers: GeneratedCodeAttribute

    private readonly AttributeSyntax _generatedCodeAttribute
        = MakeGeneratedCodeAttribute();

    private AttributeListSyntax MakeGeneratedCodeAttributeList(SyntaxNode target)
    {
        return AttributeList(
            Token(target.GetLeadingTrivia(), SyntaxKind.OpenBracketToken, default),
            target: default,
            SingletonSeparatedList(_generatedCodeAttribute),
            Token(default, SyntaxKind.CloseBracketToken, TriviaList(_endOfLine))
        );
    }

    private static AttributeSyntax MakeGeneratedCodeAttribute()
    {
        return Attribute(
            Global("System").Dot("CodeDom").Dot("Compiler").Dot("GeneratedCodeAttribute"),
            AttributeArgumentList(
                AttributeArgument(LiteralExpression(Tool.Name)),
                TokenAndSpace(SyntaxKind.CommaToken),
                AttributeArgument(LiteralExpression(Tool.Version.ToString()))
            )
        );
    }

    #endregion
}
