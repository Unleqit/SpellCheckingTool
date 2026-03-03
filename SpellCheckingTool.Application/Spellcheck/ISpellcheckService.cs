using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Spellcheck;

public interface ISpellcheckService
{
    IAlphabet Alphabet { get; }

    bool IsCorrect(Word word);

    SuggestionResult GetSuggestions(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
}