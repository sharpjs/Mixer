// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

using System.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Mixer;

/// <summary>
///   Generates a <see cref="SourceText"/> containing the code to include a
///   set of mixins into a target.
/// </summary>
internal readonly ref struct MixinOutputBuilder
{
    private readonly Target                _target;
    private readonly ImmutableArray<Mixin> _mixins;
    private readonly CancellationToken     _cancellation;

    public MixinOutputBuilder(
        Target                target,
        ImmutableArray<Mixin> mixins,
        CancellationToken     cancellation = default)
    {
        _target       = target;
        _mixins       = mixins;
        _cancellation = cancellation;
    }

    public SourceText Build()
    {
        var template = _target.GetContent(_cancellation);

        return FillTemplate(template)
            .NormalizeWhitespace()
            .GetText(Encoding.UTF8);
    }

    private CompilationUnitSyntax FillTemplate(CompilationUnitSyntax node)
    {
        // A target template compilation unit has exactly one member: a namespace
        var inner = (BaseNamespaceDeclarationSyntax) node.Members[0];

        return node.WithMembers(SingletonList(FillTemplate(inner)));
    }

    private MemberDeclarationSyntax FillTemplate(BaseNamespaceDeclarationSyntax node)
    {
        // A target template namespace has exactly one member: a type
        var inner = (TypeDeclarationSyntax) node.Members[0];

        return node.WithMembers(SingletonList(FillTemplate(inner)));
    }

    private MemberDeclarationSyntax FillTemplate(TypeDeclarationSyntax node)
    {
        // A target template type has zero or one member: a (nested) type

        if (node.Members.Any())
        {
            var inner = (TypeDeclarationSyntax) node.Members[0];

            return node.WithMembers(SingletonList(FillTemplate(inner)));
        }
        else
        {
            var mixins = GetSpecializedMixins();

            return node
                .WithAttributeLists(MergeAttributeLists(mixins))
                .WithBaseList      (MergeBaseList      (mixins))
                .WithMembers       (MergeMembers       (mixins));
        }
    }

    private ImmutableArray<TypeDeclarationSyntax> GetSpecializedMixins()
    {
        var array = ImmutableArray.CreateBuilder<TypeDeclarationSyntax>(_mixins.Length);

        foreach (var mixin in _mixins)
            array.Add(Specialize(mixin));

        return array.MoveToImmutable();
    }

    private TypeDeclarationSyntax Specialize(Mixin mixin)
    {
        var content = mixin.GetContent(_cancellation);

        return new MixinSpecializer(mixin.Type, _target.Type).Specialize(content);
    }

    private static SyntaxList<AttributeListSyntax>
        MergeAttributeLists(ImmutableArray<TypeDeclarationSyntax> mixins)
    {
        // TODO: Should this code care about conflicts?
        return List(mixins.SelectMany(m => m.AttributeLists));
    }

    private static SyntaxList<MemberDeclarationSyntax>
        MergeMembers(ImmutableArray<TypeDeclarationSyntax> mixins)
    {
        // TODO: Should this code care about conflicts?
        return List(mixins.SelectMany(m => m.Members));
    }

    private static BaseListSyntax?
        MergeBaseList(ImmutableArray<TypeDeclarationSyntax> mixins)
    {
        // TODO: Should this code care about conflicts?
        var count = 0;

        foreach (var mixin in mixins)
            if (mixin.BaseList is { Types: var types })
                foreach (var type in types)
                    if (type is not null)
                        count++;

        if (count == 0)
            return null;

        var merged = new BaseTypeSyntax[count];
        var i = 0;

        foreach (var mixin in mixins)
            if (mixin.BaseList is { Types: var types })
                foreach (var type in types)
                    if (type is not null)
                        merged[i++] = type;

        Debug.Assert(i == count);

        return BaseList(SeparatedList(merged));
    }
}
