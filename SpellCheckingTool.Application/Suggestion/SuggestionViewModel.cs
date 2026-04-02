using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Suggestion;

public class SuggestionViewModel
{
    public Word CurrentWord { get; }
    public bool IsCorrect { get; }
    public IReadOnlyList<Word> Suggestions { get; }

    public SuggestionViewModel(
        Word currentWord,
        bool isCorrect,
        IReadOnlyList<Word> suggestions,
        int startIndex)
    {
        CurrentWord = currentWord;
        IsCorrect = isCorrect;
        Suggestions = suggestions;
    }
}