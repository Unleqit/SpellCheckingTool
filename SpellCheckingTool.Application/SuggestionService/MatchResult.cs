using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.SuggestionService;

public class MatchResult
{
    Word matchWord; //changed these from private
    int distance;

    public MatchResult(Word matchWord, int distance)
    {
        this.matchWord = matchWord;
        this.distance = distance;
    }

    public Word GetMatchedWord()
    {
        return matchWord;
    }

    public int GetMatchDistance()
    {
        return distance;
    }

    public override string ToString()
    {
        return GetMatchedWord().ToString();
    }
}
