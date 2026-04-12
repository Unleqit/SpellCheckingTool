using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Suggestion;

namespace SpellCheckingTool.Application.Spellcheck;

public interface ISpellcheckService
{
    IAlphabet Alphabet { get; }

    bool IsCorrect(Word word);

    SuggestionResult GetSuggestions(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
}