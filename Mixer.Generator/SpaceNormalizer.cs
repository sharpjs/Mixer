// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

using static Math;
using static MathEx;

/// <summary>
///   A C# syntax rewriter that normalizes indentation and newlines.
/// </summary>
internal class SpaceNormalizer : CSharpSyntaxRewriter
{
    private State      _state;  // overall state
    private int        _shift;  // count of spaces to +add to indentation (may be negative)
    private string?[]? _spaces; // indentation string cache

    private readonly SyntaxTrivia _endOfLine; // end-of-line trivia

    private enum State
    {
        Initial,     // at start of text            => discard newlines and reindent
        StartOfLine, // at start of subsequent line => reindent
        Interior     // in interior of line         => do nothing
    }

    /// <summary>
    ///   Initializes a new <see cref="SpaceNormalizer"/> instance.
    /// </summary>
    public SpaceNormalizer()
        : base(visitIntoStructuredTrivia: true)
    {
        _endOfLine = GetPlatformEndOfLine();
    }

    public T Normalize<T>(T node, int indent)
        where T : SyntaxNode
    {
        Reset(node, indent);

        return (T) Visit(node)!;
    }

    public SyntaxList<T> Normalize<T>(SyntaxList<T> nodes, int indent)
        where T : SyntaxNode
    {
        if (!nodes.Any())
            return nodes;

        Reset(nodes[0], indent);

        return VisitList(nodes);
    }

    private void Reset(SyntaxNode node, int indent)
    {
        if (indent < 0)
            throw new ArgumentOutOfRangeException(nameof(indent));

        _state = State.Initial;
        _shift = indent - DetectIndent(node);
    }

    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        // Replace the base implementation with one that synthesizes fake
        // leading trivia to indent if the token does not have any.

        var leadingTrivia  = VisitListOrDefault(token.LeadingTrivia);
        var trailingTrivia = VisitList         (token.TrailingTrivia);

        if (leadingTrivia != token.LeadingTrivia)
            token = token.WithLeadingTrivia(leadingTrivia);

        if (trailingTrivia != token.TrailingTrivia)
            token = token.WithTrailingTrivia(trailingTrivia);

        return token;
    }

    private SyntaxTriviaList VisitListOrDefault(SyntaxTriviaList list)
    {
        if (list.Any())
            return VisitList(list);

        // The token might occur at the start of a line but without any leading
        // trivia.  In that case, visit a fake whitespace trivia to allow this
        // rewriter a chance to synthesize any necessary indentation.

        var trivia = VisitWhitespaceTrivia(default);

        return trivia == default
            ? list
            : TriviaList(trivia);
    }

    public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
    {
        return trivia.Kind() switch
        {
            SyntaxKind.WhitespaceTrivia => VisitWhitespaceTrivia(trivia),
            SyntaxKind.EndOfLineTrivia  => VisitEndOfLineTrivia (trivia),
            _                           => VisitOtherTrivia     (trivia),
        };
    }

    private SyntaxTrivia VisitWhitespaceTrivia(SyntaxTrivia trivia)
    {
        // Do not care about space in line interior
        if (_state == State.Interior)
            return trivia;

        // Assume indentation is one trivia node; subsequent nodes are interior
        _state = State.Interior;

        // Skip reindenting if possible
        if (_shift == 0)
            return trivia;

        // Synthesize new indentation
        return Reindent(trivia);
    }

    private SyntaxTrivia VisitEndOfLineTrivia(SyntaxTrivia trivia)
    {
        // Strip line endings at start of text
        if (_state == State.Initial)
            return default;

        // Every other line ending begins a new line
        _state = State.StartOfLine;

        // Skip replacing line ending if possible
        if (trivia.IsEquivalentTo(_endOfLine))
            return trivia;

        // Replace line ending
        return _endOfLine;
    }

    private SyntaxTrivia VisitOtherTrivia(SyntaxTrivia trivia)
    {
        // Recurse into structured trivia
        if (trivia.HasStructure)
            return base.VisitTrivia(trivia);

        // Non-space trivia cannot be indentation; must be interior
        _state = State.Interior;
        return trivia;
    }

    private SyntaxTrivia Reindent(SyntaxTrivia trivia)
    {
        var length = GetVisualLength(trivia.ToString()) + _shift;
        var space  = GetSpace(length);
        return Whitespace(space);
    }

    private string GetSpace(int length)
    {
        var spaces = EnsureSpaceCacheSlot(length);
        return spaces[length] ??= new string(' ', length);
    }

    private string?[] EnsureSpaceCacheSlot(int index)
    {
        const int MinimumLength = 4;

        if (_spaces is not { } spaces)
        {
            spaces = new string?[GetSpaceCacheSize(index + 1, MinimumLength)];
            return _spaces = spaces;
        }
        else if (spaces.Length <= index)
        {
            Array.Resize(ref spaces, GetSpaceCacheSize(index, spaces.Length * 2));
            return _spaces = spaces;
        }
        else
        {
            return spaces;
        }
    }

    private static int GetSpaceCacheSize(int requested, int minimum)
    {
        return Max(RoundUpToPowerOf2(requested), minimum);
    }

    private static int DetectIndent(SyntaxNode node)
    {
        foreach (var trivia in node.GetLeadingTrivia())
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.WhitespaceTrivia:
                    return GetVisualLength(trivia.ToString());

                case SyntaxKind.EndOfLineTrivia:
                    continue;

                default:
                    return 0;
            }
        }

        return 0;
    }

    private static int GetVisualLength(string s)
    {
        var length = 0;

        foreach (var c in s)
            length += c == '\t' ? GetDistanceToNextIndentStop(length) : 1;

        return length;
    }

    private static int GetDistanceToNextIndentStop(int column)
    {
        return IndentSize - column % IndentSize;
    }
}
