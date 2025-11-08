namespace SpellCheckingTool
{
    public interface ISuggestionService
    {
        public SuggestionResult GetSuggestionResult(string input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
    }
}
