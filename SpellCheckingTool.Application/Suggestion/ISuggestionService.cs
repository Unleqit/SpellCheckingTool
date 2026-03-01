using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Suggestion;

public interface ISuggestionService
{
    public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
}
