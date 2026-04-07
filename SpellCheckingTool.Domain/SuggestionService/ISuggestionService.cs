using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Domain.Suggestion;

public interface ISuggestionService
{
    public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
}
