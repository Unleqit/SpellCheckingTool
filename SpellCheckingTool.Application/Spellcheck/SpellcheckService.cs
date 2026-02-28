using SpellCheckingTool.Application.SuggestionService;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Spellcheck;

public class SpellcheckService : ISpellcheckService
{
    private readonly WordTree tree;
    private readonly ISuggestionService suggestionService;

    public SpellcheckService(WordTree tree, ISuggestionService suggestionService)
    {
        this.tree = tree;
        this.suggestionService = suggestionService;
    }

    public IAlphabet Alphabet => tree.Alphabet;

    public bool IsCorrect(Word word)
    {
        return tree.Contains(word);
    }

    public SuggestionResult GetSuggestions(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance)
    {
        // Now routed through the Application boundary interface
        return suggestionService.GetSuggestionResult(input, maxAmountOfSuggestionsToBeReturned, maxAllowedDistance);
    }
}