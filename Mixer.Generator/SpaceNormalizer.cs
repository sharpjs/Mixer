// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

using static Math;
using static MathEx;

/// <summary>
///   A C# syntax rewriter that normalizes indentation and line endings.
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

    /// <summary>
    ///   Normalizes the indentation and line endings of the specified node.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of <paramref name="node"/>.
    /// </typeparam>
    /// <param name="node">
    ///   The node to normalize.
    /// </param>
    /// <param name="indent">
    ///   The desired indentation, in columns.
    /// </param>
    /// <returns>
    ///   <paramref name="node"/> shifted left or right so that it is indented
    ///   by <paramref name="indent"/> columns, and with line endings replaced
    ///   by the platform's preferred line ending.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="indent"/> is negative.
    /// </exception>
    public T Normalize<T>(T node, int indent)
        where T : SyntaxNode
    {
        if (indent < 0)
            throw new ArgumentOutOfRangeException(nameof(indent));

        Reset(node, indent);

        return (T) Visit(node)!;
    }

    /// <summary>
    ///   Normalizes the indentation and line endings of each node in the
    ///   specified list.
    /// </summary>
    /// <typeparam name="T">
    ///   The type of element in <paramref name="nodes"/>.
    /// </typeparam>
    /// <param name="nodes">
    ///   The list of nodes to normalize.
    /// </param>
    /// <param name="indent">
    ///   The desired indentation, in columns.
    /// </param>
    /// <returns>
    ///   <paramref name="nodes"/>, each shifted left or right so that it is
    ///   indented by <paramref name="indent"/> columns, and each with line
    ///   endings replaced by the platform's preferred line ending.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="indent"/> is negative.
    /// </exception>
    public SyntaxList<T> Normalize<T>(SyntaxList<T> nodes, int indent)
        where T : SyntaxNode
    {
        if (indent < 0)
            throw new ArgumentOutOfRangeException(nameof(indent));

        if (!nodes.Any())
            return nodes;

        Reset(nodes[0], indent);

        return VisitList(nodes);
    }

#if WANT_TO_NORMALIZE_A_TOKEN
    public SyntaxToken Normalize(SyntaxToken token, int indent)
    {
        if (indent < 0)
            throw new ArgumentOutOfRangeException(nameof(indent));

        Reset(token, indent);

        return VisitToken(token);
    }
#endif

    /// <summary>
    ///   Normalizes the indentation and line endings of each element in the
    ///   specified trivia list.
    /// </summary>
    /// <param name="trivia">
    ///   The list of trivia to normalize.
    /// </param>
    /// <param name="indent">
    ///   The desired indentation, in columns.
    /// </param>
    /// <returns>
    ///   <paramref name="trivia"/>, each shifted left or right so that it is
    ///   indented by <paramref name="indent"/> columns, and each with line
    ///   endings replaced by the platform's preferred line ending.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///   <paramref name="indent"/> is negative.
    /// </exception>
    public SyntaxTriviaList Normalize(SyntaxTriviaList trivia, int indent)
    {
        if (indent < 0)
            throw new ArgumentOutOfRangeException(nameof(indent));

        Reset(trivia, indent);

        return trivia.Any() ? VisitListCore(trivia) : trivia;
    }

    private void Reset(SyntaxNode node, int indent)
    {
        Reset(node.GetLeadingTrivia(), indent);
    }

#if WANT_TO_NORMALIZE_A_TOKEN
    private void Reset(SyntaxToken token, int indent)
    {
        Reset(token.LeadingTrivia, indent);
    }
#endif

    private void Reset(SyntaxTriviaList trivia, int indent)
    {
        _state = State.Initial;
        _shift = indent - DetectIndent(trivia);
    }

    public override SyntaxToken VisitToken(SyntaxToken token)
    {
        if (token.IsKind(SyntaxKind.None))
            return token;

        var leadingTrivia = VisitLeadingTrivia(token.LeadingTrivia);

        if (token.Span.Length > 0)
            _state = State.Interior; // due to the token itself

        var trailingTrivia = VisitTrailingTrivia(token.TrailingTrivia);

        if (leadingTrivia != token.LeadingTrivia)
            token = token.WithLeadingTrivia(leadingTrivia);

        if (trailingTrivia != token.TrailingTrivia)
            token = token.WithTrailingTrivia(trailingTrivia);

        return token;
    }

    private SyntaxTriviaList VisitLeadingTrivia(SyntaxTriviaList list)
    {
        if (list.Any())
            return VisitListCore(list);

        // The token might occur at the start of a line without any preceding
        // whitespace trivia.  To handle that case, visit some imaginary
        // preceding whitespace trivia to allow the rewriter a chance to
        // synthesize any necessary indentation and update state.
        var newTrivia = VisitWhitespaceTrivia();

        return newTrivia.IsKind(SyntaxKind.None)
            ? list
            : TriviaList(newTrivia);
    }

    public SyntaxTriviaList VisitTrailingTrivia(SyntaxTriviaList list)
    {
        if (list.Any())
            return VisitListCore(list);

        return list;
    }

    private SyntaxTriviaList VisitListCore(SyntaxTriviaList list)
    {
        var editor = new SyntaxTriviaListEditor(list);

        foreach (var oldTrivia in list)
        {
            var newTrivia = oldTrivia.Kind() switch
            {
                SyntaxKind.WhitespaceTrivia => VisitWhitespaceTrivia(oldTrivia),
                SyntaxKind.EndOfLineTrivia  => VisitEndOfLineTrivia (oldTrivia),
                _                           => VisitOtherTrivia     (oldTrivia, editor),
            };

            var different = !newTrivia.IsEquivalentTo(oldTrivia) || oldTrivia.IsNone();
            editor.Add(newTrivia, different);
        }

        return editor.ToList();
    }

    private SyntaxTrivia VisitWhitespaceTrivia(SyntaxTrivia trivia = default)
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

    private SyntaxTrivia VisitOtherTrivia(SyntaxTrivia trivia, SyntaxTriviaListEditor editor)
    {
        // Recurse into structured trivia to visit its leaf nodes
        if (trivia.HasStructure)
            return VisitTrivia(trivia);

        // Other unstructured trivia might occur at the start of a line without
        // any preceding whitespace trivia.  To handle that case, visit some
        // imaginary preceding whitespace trivia to allow the rewriter a chance
        // to synthesize any necessary indentation and update state.
        var space = VisitWhitespaceTrivia();
        editor.Add(space, different: space.IsSome());

        // Other unstructured trivia is neither whitespace nor a newline, so
        // there is no rewriting to do for it
        return trivia;
    }

    private SyntaxTrivia Reindent(SyntaxTrivia trivia)
    {
        var length = GetVisualLength(trivia.ToString()) + _shift;
        if (length < 1)
            return default;

        var space = GetSpace(length);
        return Whitespace(space);
    }

    private string GetSpace(int length)
    {
        var cache = EnsureSpaceCacheSlot(length);
        return cache[length] ??= new string(' ', length);
    }

    private string?[] EnsureSpaceCacheSlot(int index)
    {
        if (_spaces is not { } spaces)
            return _spaces = CreateSpaceCache(index + 1);
        else if (spaces.Length <= index)
            return _spaces = GrowSpaceCache(spaces, index + 1);
        else
            return spaces;
    }

    private string?[] CreateSpaceCache(int size)
    {
        const int MinimumLength = 4;

        size = GetSpaceCacheSize(size, MinimumLength);
        return new string?[size];
    }

    private string?[] GrowSpaceCache(string?[] cache, int size)
    {
        size = GetSpaceCacheSize(size, cache.Length * 2);
        Array.Resize(ref cache, size);
        return cache;
    }

    private static int GetSpaceCacheSize(int requested, int minimum)
    {
        return Max(RoundUpToPowerOf2(requested), minimum);
    }

    private static int DetectIndent(SyntaxTriviaList leadingTrivia)
    {
        foreach (var trivia in leadingTrivia)
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
