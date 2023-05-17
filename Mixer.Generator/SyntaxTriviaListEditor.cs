// Copyright 2023 Subatomix Research Inc.
// SPDX-License-Identifier: ISC

namespace Mixer;

/// <summary>
///   Helper that implements copy-on-write behavior for sequential processing
///   of a <see cref="SyntaxTriviaList"/>.
/// </summary>
internal ref struct SyntaxTriviaListEditor
{
    private readonly SyntaxTriviaList _oldList;
    private List<SyntaxTrivia>?       _newList;
    private int                       _equalCount; // or -1 if difference has occurred

    public SyntaxTriviaListEditor(SyntaxTriviaList list)
    {
        _oldList = list;
    }

    /// <summary>
    ///   Indicates that the next element from the original list will not be
    ///   added to the edited list.
    /// </summary>
    public void Skip()
    {
        CopyOnWrite();
    }

    /// <summary>
    ///   Adds the specified trivia to the edited list.  The trivia is assumed
    ///   to be the next element from the original list.
    /// </summary>
    /// <param name="trivia">
    ///   The next trivia from the original list.
    /// </param>
    public void Copy(SyntaxTrivia trivia)
    {
        Add(trivia, equal: true);
    }

    /// <summary>
    ///   Adds the specified trivia to the edited list.  The trivia is assumed
    ///   to be new, <b>not</b> be the next element from the original list.
    /// </summary>
    /// <param name="trivia">
    ///   The new trivia to add.
    /// </param>
    public void Add(SyntaxTrivia trivia)
    {
        Add(trivia, equal: false);
    }

    /// <summary>
    ///   Adds the specified trivia to the edited list.  The caller supplies
    ///   the next element from the original list for comparison.
    /// </summary>
    /// <param name="trivia">
    ///   The trivia to add.
    /// </param>
    /// <param name="original">
    ///   The next element from the original list.
    /// </param>
    public void Add(SyntaxTrivia trivia, SyntaxTrivia original)
    {
        Add(trivia, equal: trivia.IsEquivalentTo(original));
    }

    private void Add(SyntaxTrivia trivia, bool equal)
    {
        // Until difference is found, increment the count of equal items
        if (AccumulateEquality(equal))
            _equalCount++;

        // Exclude default trivia from result
        else if (trivia.IsSome())
            EnsureList().Add(trivia);
    }

    private bool AccumulateEquality(bool equal)
    {
        if (_equalCount < 0)
            return false;

        if (equal)
            return true;

        CopyOnWrite();
        return false;
    }

    private void CopyOnWrite()
    {
        _newList    = Copy(_oldList, _equalCount);
        _equalCount = -1;
    }

    private List<SyntaxTrivia> EnsureList()
    {
        return _newList ??= new();
    }

    private static List<SyntaxTrivia>? Copy(SyntaxTriviaList source, int count)
    {
        if (count == 0)
            return null;

        var target = new List<SyntaxTrivia>();
        var index  = 0;

        foreach (var item in source)
        {
            target.Add(item);

            if (++index >= count)
                break;
        }

        return target;
    }

    /// <summary>
    ///   Gets the edited list.
    /// </summary>
    public SyntaxTriviaList ToList()
    {
        if (_equalCount >= 0)
            return _oldList;

        return _newList is null
            ? TriviaList()
            : TriviaList(_newList);
    }
}
