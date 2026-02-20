namespace SpellCheckingTool
{
    public interface ISuggestionService
    {
        public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
    }
}
