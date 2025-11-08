namespace SpellCheckingTool
{
    public class SuggestionResult
    {
        string[] suggestions;
        int length;

        public SuggestionResult(string[] suggestions, int length)
        {
            this.suggestions = suggestions;
            this.length = length;
        }

        public int GetSuggestionCount()
        {
            return length;
        }

        public string[] GetSuggestionArray()
        {
            return suggestions;
        }

        public List<string> GetSuggestionList()
        {
            return suggestions.ToList();
        }
    }
}
