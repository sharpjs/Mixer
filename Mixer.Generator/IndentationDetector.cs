// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

internal ref struct IndentationDetector
{
    private bool         _isStartOfLine;
    private SyntaxTrivia _indentation;

    public IndentationDetector()
    {
        _isStartOfLine = true;
        _indentation   = default;
    }

    public static SyntaxTrivia Detect(SyntaxTriviaList trivia)
    {
        var detector = new IndentationDetector();
        detector.Visit(trivia);
        return detector._indentation;
    }

    private void Visit(SyntaxTriviaList list)
    {
        foreach (var trivia in list)
            Visit(trivia);
    }

    private void Visit(IEnumerable<SyntaxTrivia> enumerable)
    {
        foreach (var trivia in enumerable)
            Visit(trivia);
    }

    private void Visit(SyntaxTrivia trivia)
    {
        switch (trivia.Kind())
        {
            case SyntaxKind.WhitespaceTrivia when _isStartOfLine:
                _isStartOfLine = false;
                _indentation   = trivia;
                break;

            case SyntaxKind.EndOfLineTrivia:
                _isStartOfLine = true;
                _indentation   = default;
                break;

            case var _ when trivia.HasStructure:
                Visit(trivia.GetStructure()!.DescendantTrivia(descendIntoTrivia: true));
                break;

            default:
                _isStartOfLine = false;
                _indentation   = default;
                break;

        }
    }
}
