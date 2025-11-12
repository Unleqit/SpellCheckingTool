namespace SpellCheckingTool
{
    public interface ISuggestionService : IDisposable
    {
        public SuggestionResult GetSuggestionResult(string input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
    }
}
