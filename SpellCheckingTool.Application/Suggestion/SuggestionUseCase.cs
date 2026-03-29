using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Domain.WordTree;


namespace SpellCheckingTool.Application.Suggestion
{
    public class SuggestionUseCase
{
    private readonly ISpellcheckService _defaultService;
    private readonly ISpellcheckService _executableService;

    public int MaxSuggestions { get; set; }
    public int MaxDistance { get; set; }

        public SuggestionUseCase(ISpellcheckService defaultService, ISpellcheckService executableService)
        {
            _defaultService = defaultService;
            _executableService = executableService;
            MaxSuggestions = 5;
            MaxDistance = 2;
        }

        public SuggestionViewModel Execute(string input)
        {
            var (wordString, startIndex) = ExtractLastWord(input);
            var isFirstWord = IsFirstWord(input);

            var activeService = isFirstWord
                ? _executableService
                : _defaultService;

            var word = new Word(activeService.Alphabet, wordString);

            bool isCorrect = activeService.IsCorrect(word);
            var suggestionsResult = activeService.GetSuggestions(word, MaxSuggestions, MaxDistance);

            if (!isCorrect && isFirstWord && _defaultService.IsCorrect(word))
            {
                isCorrect = true;
                suggestionsResult = _defaultService.GetSuggestions(word, MaxSuggestions, MaxDistance);
            }

            var suggestions = suggestionsResult.GetSuggestionArray();

            return new SuggestionViewModel(
                word,
                isCorrect,
                suggestions,
                startIndex);
        }

        private static (string wordString, int startIndex) ExtractLastWord(string input)
        {
            int startIndex = input.LastIndexOf(' ') + 1;
            string wordString = input.Substring(startIndex);
            return (wordString, startIndex);
        }

        private static bool IsFirstWord(string input)
        {
            var tokens = input.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return tokens.Length <= 1;
        }
    }
}
