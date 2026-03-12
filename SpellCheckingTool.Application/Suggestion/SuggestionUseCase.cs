using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Domain.WordTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Application.Suggestion
{
    public class SuggestionUseCase
{
    private readonly ISpellcheckService _spellcheckService;

    public int MaxSuggestions { get; set; }
    public int MaxDistance { get; set; }

        public SuggestionUseCase(ISpellcheckService spellcheckService)
        {
            _spellcheckService = spellcheckService;
        }

        public SuggestionViewModel Execute(string input)
        {
            int startIndex = input.LastIndexOf(' ') + 1;

            string wordString = input.Substring(startIndex);

            Word word = new Word(_spellcheckService.Alphabet, wordString);

            bool isCorrect = _spellcheckService.IsCorrect(word);

            var suggestions =
                _spellcheckService
                    .GetSuggestions(word, MaxSuggestions, MaxDistance)
                    .GetSuggestionArray();

            return new SuggestionViewModel(
                word,
                isCorrect,
                suggestions,
                startIndex);
        }
    }
}
