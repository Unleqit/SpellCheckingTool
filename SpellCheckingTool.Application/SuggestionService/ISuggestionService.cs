using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.SuggestionService;

public interface ISuggestionService
{
    public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
}
