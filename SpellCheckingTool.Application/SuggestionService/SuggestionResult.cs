using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.SuggestionService;

public class SuggestionResult
{
    private readonly Word[] results;
    private readonly int suggestionCount;
    private readonly int totalMatchCount;

    public SuggestionResult(Word[] results, int suggestionCount, int totalMatchCount)
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

    public Word[] GetSuggestionArray()
    {
        return results;
    }
}