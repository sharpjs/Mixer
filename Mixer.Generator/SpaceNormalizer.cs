// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

using static Math;
using static MathEx;

/// <summary>
///   A C# syntax rewriter that normalizes indentation and newlines.
/// </summary>
/// <remarks>
///   This rewriter assumes that the developer prefers a virtual tab stop every
///   four columns indented with spaces, not tabs, as is common practice in the
///   C# community.  It is beyond the scope of this generator to honor
///   alternative preferences, per the guidance
///   <a href="https://stackoverflow.com/questions/67351269/can-a-roslyn-source-generator-discover-the-ides-spacing-etc-preferences">here</a>
///   and
///   <a href="https://github.com/dotnet/roslyn/issues/53020">here</a>.
/// </remarks>
internal class SpaceNormalizer : CSharpSyntaxRewriter
{
    private const ushort TabStop = 4;

    private          State        _state;       // line state
    private          int          _shift;       // number of columns to shift each line rightward
    private          string?[]?   _spaces;      // indentation string cache
    private readonly SyntaxTrivia _endOfLine;   // end-of-line trivia

    private enum State : byte
    {
        Initial,        // at start of first line
        LineStart,      // at start of successive line
        LineInterior,   // not at start of line
    }

    public SpaceNormalizer()
        : base(visitIntoStructuredTrivia: true)
    {
        _endOfLine = GetPlatformEndOfLine();
    }

    public T Normalize<T>(T node, int indent)
        where T : SyntaxNode
    {
        if (indent < 0)
            throw new ArgumentOutOfRangeException(nameof(indent));

        Reset(node, indent);

        return (T) Visit(node)!;
    }

    public SyntaxList<T> Normalize<T>(SyntaxList<T> nodes, int indent)
        where T : SyntaxNode
    {
        if (indent < 0)
            throw new ArgumentOutOfRangeException(nameof(indent));

        if (nodes.Count == 0)
            return nodes;

        Reset(nodes[0], indent);

        return VisitList(nodes);
    }

    private void Reset(SyntaxNode node, int indent)
    {
        _state = State.Initial;
        _shift = indent - DetectIndent(node);
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
        // Do not care about interior space
        if (_state == State.LineInterior)
            return trivia;

        // Assume no more indentation in current line
        _state = State.LineInterior;

        // Avoid reindenting if possible
        if (_shift == 0)
            return trivia;

        // Synthesize new indentation
        return Reindent(trivia);
    }

    private SyntaxTrivia VisitEndOfLineTrivia(SyntaxTrivia trivia)
    {
        // Strip leading line endings
        if (_state == State.Initial)
            return default;

        // Every line ending begins a new line
        _state = State.LineStart;

        // Avoid replacing line ending if possible
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

        // Assume no more indentation in current line
        _state = State.LineInterior;
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
        var spaces = EnsureSpaceCache(length);
        return spaces[length] ??= new string(' ', length);
    }

    private string?[] EnsureSpaceCache(int length)
    {
        const int MinimumLength = 4;

        if (_spaces is not { } spaces)
        {
            spaces = new string?[GetPreferredLength(length, MinimumLength)];
            return _spaces = spaces;
        }
        else if (spaces.Length <= length)
        {
            Array.Resize(ref spaces, GetPreferredLength(length, spaces.Length * 2));
            return _spaces = spaces;
        }
        else
        {
            return spaces;
        }
    }

    private static int DetectIndent(SyntaxNode node)
    {
        var indent = 0;

        foreach (var trivia in node.GetLeadingTrivia())
        {
            switch (trivia.Kind())
            {
                case SyntaxKind.WhitespaceTrivia:
                    indent += GetVisualLength(trivia.ToString());
                    break;

                case SyntaxKind.EndOfLineTrivia:
                    indent = 0;
                    break;

                default:
                    return indent;
            }
        }

        return indent;
    }

    private static int GetVisualLength(string s)
    {
        var length = 0;

        foreach (var c in s)
            length += c == '\t' ? GetDistanceToNextTabStop(length) : 1;

        return length;
    }

    private static int GetDistanceToNextTabStop(int column)
    {
        return TabStop - column % TabStop;
    }

    private static int GetPreferredLength(int requested, int minimum)
    {
        return Max(RoundUpToPowerOf2(requested), minimum);
    }
}
