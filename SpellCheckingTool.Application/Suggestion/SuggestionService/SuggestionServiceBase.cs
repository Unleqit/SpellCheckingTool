using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Suggestion;

public abstract class SuggestionServiceBase : ISuggestionService
{
    private readonly IDistanceAlgorithm distanceAlgorithm;
    private readonly LeftToRightWordTreeTraversal traversal;


    public SuggestionServiceBase(WordTree tree, IDistanceAlgorithm distanceAlgorithm)
    {
        this.distanceAlgorithm = distanceAlgorithm;
        this.traversal = new LeftToRightWordTreeTraversal(tree);
    }

    /// <summary>
    /// Returns an object holding the best fit matches determined by the DistanceAlgorithm used in this instance.
    /// </summary>
    public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned = 3, int maxAllowedDistance = 4)
    {
        int maxSuggestions = Math.Clamp(maxAmountOfSuggestionsToBeReturned, 0, 20);
        int matchesCount = 0;
        int totalMatchesCount = 0;
        int indexOfMatchToBeReplacedNext = 0;
        double normalizedDistance = 0;
        double normalizedWorstDistanceValueInResults = 1;
        MatchResult[] matchResults = new MatchResult[maxSuggestions];

        Prewalk();

        traversal.WalkTree((word) =>
        {
            normalizedDistance = ComputeNormalizedDistance(input, word, distanceAlgorithm, maxAllowedDistance);

            if (normalizedDistance >= 0 && normalizedDistance < normalizedWorstDistanceValueInResults)
            {
                matchResults[indexOfMatchToBeReplacedNext] = new MatchResult(word, normalizedDistance);
                totalMatchesCount++;

                if (totalMatchesCount < maxSuggestions)
                {
                    indexOfMatchToBeReplacedNext++;
                }
                else
                {
                    normalizedWorstDistanceValueInResults = 0;
                    for (int i = 0; i < maxSuggestions; ++i)
                        if (matchResults[i].GetMatchDistance() > normalizedWorstDistanceValueInResults)
                            normalizedWorstDistanceValueInResults = matchResults[indexOfMatchToBeReplacedNext = i].GetMatchDistance();
                }
            }
        });

        matchesCount = Math.Min(totalMatchesCount, maxSuggestions);

        SortMatches(matchResults, matchesCount);

        Word[] matchedWords = matchResults.Where(entry => entry != null).Select(entry => entry.GetMatchedWord()).ToArray();
        return new SuggestionResult(matchedWords, matchesCount, totalMatchesCount);
    }

    protected abstract double ComputeNormalizedDistance(Word input, Word current, IDistanceAlgorithm distanceAlgorithm, int maxAllowedDistance);

    protected virtual void Prewalk()
    {

    }

    /// <summary>
    /// bubble sorts the matches array by their distances in ascending order
    /// </summary>
    private void SortMatches(MatchResult[] results, int matchesCount)
    {
        MatchResult resultToBeSwapped;

        for (int i = 0; i < matchesCount - 1; ++i)
            for (int j = 0; j < matchesCount - i - 1; ++j)
                if (results[j].GetMatchDistance() > results[j + 1].GetMatchDistance())
                {
                    resultToBeSwapped = results[j];
                    results[j] = results[j + 1];
                    results[j + 1] = resultToBeSwapped;
                }
    }

    // Implementation detail: match buffer entry for selecting best suggestions
    private class MatchResult
    {
        private readonly Word matchWord;
        private readonly double distance;

        public MatchResult(Word matchWord, double distance)
        {
            this.matchWord = matchWord;
            this.distance = distance;
        }

        public Word GetMatchedWord()
        {
            return matchWord;
        }

        public double GetMatchDistance()
        {
            return distance;
        }

        public override string ToString()
        {
            return matchWord.ToString();
        }
    }
}