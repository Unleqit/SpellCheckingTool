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

    public SuggestionUseCase(ISpellcheckService spellcheckService, int maxSuggestions, int maxDistance)
    {
        _spellcheckService = spellcheckService;
        MaxSuggestions = maxSuggestions;
        MaxDistance = maxDistance;
        }

        public SuggestionUseCase(ISpellcheckService spellcheckService)
        {
            _spellcheckService = spellcheckService;
        }

        public SuggestionViewModel Execute(Word word)
    {
        bool isCorrect = _spellcheckService.IsCorrect(word);

        var suggestions =
            _spellcheckService
                .GetSuggestions(word, MaxSuggestions, MaxDistance)
                .GetSuggestionArray();

        return new SuggestionViewModel(
            word,
            isCorrect,
            suggestions);
    }
}
}
