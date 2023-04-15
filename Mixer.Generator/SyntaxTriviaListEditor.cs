// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

internal ref struct SyntaxTriviaListEditor
{
    private List<SyntaxTrivia>?       _newList;
    private readonly SyntaxTriviaList _oldList;
    private bool                      _different;   // if encountered difference(s)
    private int                       _countToCopy; // on encountering first difference

    public SyntaxTriviaListEditor(SyntaxTriviaList list)
    {
        _oldList = list;
    }

    public void Add(SyntaxTrivia trivia, bool different)
    {
        // Until difference is found, increment the count of identical items
        if (!(_different |= different))
            _countToCopy++;

        // Exclude default trivia from result
        else if (trivia.IsSome())
            EnsureList().Add(trivia);
    }

    private List<SyntaxTrivia> EnsureList()
    {
        if (_newList is not null)
            return _newList;

        Copy(_oldList, _newList = new(), _countToCopy);
        return _newList;
    }

    private static void Copy(SyntaxTriviaList source, List<SyntaxTrivia> target, int count)
    {
        var index = 0;

        foreach (var item in source)
            if (index++ < count)
                target.Add(item);
    }

    public SyntaxTriviaList ToList()
    {
        if (!_different)
            return _oldList;

        return _newList is null
            ? TriviaList()
            : TriviaList(_newList);
    }
}