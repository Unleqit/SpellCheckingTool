namespace SpellCheckingTool
{
    public unsafe struct SuggestionResult
    {
        string[] results;
        int suggestionCount;
        int totalMatchCount;

        public SuggestionResult(string[] results, int suggestionCount, int totalMatchCount)
        {
            this.results = results;
            this.suggestionCount = suggestionCount;
            this.totalMatchCount = totalMatchCount;
        }

        public int GetSuggestionCount()
        {
            return suggestionCount;
        }

        public int GetTotalMatchCount()
        {
            return totalMatchCount;
        }

        public string[] GetSuggestionArray()
        {
            return results;
        }
    }
}
