using SpellCheckingTool.Domain.WordTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Application.Suggestion;

public class SuggestionViewModel
{
    public Word CurrentWord { get; }
    public bool IsCorrect { get; }
    public IReadOnlyList<Word> Suggestions { get; }

    public SuggestionViewModel(
        Word currentWord,
        bool isCorrect,
        IReadOnlyList<Word> suggestions)
    {
        CurrentWord = currentWord;
        IsCorrect = isCorrect;
        Suggestions = suggestions;
    }
}