using SpellCheckingTool.Application.Spellcheck;
using SpellCheckingTool.Domain.WordTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Application.Suggestion
{
    public class SuggestionController
    {
        private readonly ISpellcheckService spellcheckService;

        public SuggestionController(ISpellcheckService spellcheckService)
        {
            this.spellcheckService = spellcheckService;
        }

        public SuggestionViewModel BuildViewModel(
            Word word,
            int maxSuggestions,
            int maxDistance)
        {
            bool isCorrect = spellcheckService.IsCorrect(word);

            var suggestionResult =
                spellcheckService.GetSuggestions(word, maxSuggestions, maxDistance);

            return new SuggestionViewModel(
                word,
                isCorrect,
                suggestionResult.GetSuggestionArray());
        }
    }
}