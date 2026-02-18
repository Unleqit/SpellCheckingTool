namespace SpellCheckingTool
{
    public interface ISuggestionService : IDisposable
    {
        public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance);
    }
}
