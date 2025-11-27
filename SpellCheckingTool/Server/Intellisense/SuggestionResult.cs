namespace SpellCheckingTool
{
    public unsafe struct SuggestionResult
    {
        char** suggestions;
        int* suggestionLengths;
        int length;

        public SuggestionResult(char** suggestions, int* suggestionLengths, int length)
        {
            this.suggestions = suggestions;
            this.suggestionLengths = suggestionLengths;
            this.length = length;
        }

        public int GetSuggestionCount()
        {
            return length;
        }

        public int* GetSuggestionLengths()
        {
            return suggestionLengths;
        }

        public char** GetSuggestionArray()
        {
            return suggestions;
        }

        /// <summary>
        /// Convenience method. Use GetSuggestionArray() and GetSuggestionLengths() in performance-critical scenarios instead.
        /// </summary>
        public string[] GetSuggestionArrayManaged()
        {
            string[] tmp = new string[length];

            for (int i = 0; i < length; ++i)
                for (int j = 0; j < *(suggestionLengths + i); ++j)
                    tmp[i] += suggestions[i][j];

            return tmp;
        }
    }
}
