using SpellCheckingTool.Application.SuggestionService;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Spellcheck;

public interface ISpellcheckService
{
    IAlphabet Alphabet { get; }

    bool IsCorrect(Word word);

    // Keep signature similar to your old flow. For now it can return empty results.
    SuggestionResult GetSuggestions(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
}