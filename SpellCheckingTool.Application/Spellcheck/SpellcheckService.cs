using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Spellcheck;

public class SpellcheckService : ISpellcheckService
{
    private readonly IWordStorage tree;
    private readonly ISuggestionService suggestionService;
    private readonly IAlphabet inputAlphabet;

    public SpellcheckService(IWordStorage tree, ISuggestionService suggestionService, IAlphabet inputAlphabet)
    {
        this.tree = tree;
        this.suggestionService = suggestionService;
        this.inputAlphabet = inputAlphabet;
    }

    public IAlphabet Alphabet => inputAlphabet;

    public bool IsCorrect(Word word)
    {
        return tree.Contains(word);
    }

    public SuggestionResult GetSuggestions(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance)
    {
        return suggestionService.GetSuggestionResult(input, maxAmountOfSuggestionsToBeReturned, maxAllowedDistance);
    }
}