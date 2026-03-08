using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Suggestion;

public class SuggestionService : ISuggestionService
{
    private readonly WordTree tree;
    private readonly IDistanceAlgorithm distanceAlgorithm;
    private readonly LeftToRightWordTreeTraversal traversal;

    public SuggestionService(WordTree tree, IDistanceAlgorithm distanceAlgorithm)
    {
        this.tree = tree;
        this.distanceAlgorithm = distanceAlgorithm;
        traversal = new LeftToRightWordTreeTraversal(tree);
    }

    /// <summary>
    /// Returns an object holding the best fit matches determined by the DistanceAlgorithm used in this instance.
    /// </summary>
    public SuggestionResult GetSuggestionResult(
        Word input,
        int maxAmountOfSuggestionsToBeReturned = 3,
        int maxAllowedDistance = 4)
    {
        int maxSuggestions = Math.Clamp(maxAmountOfSuggestionsToBeReturned, 0, 20);

        int distanceToInputWord = 0;
        int matchesCount = 0;
        int totalMatchesCount = 0;
        int indexOfMatchToBeReplacedNext = 0;

        MatchResult[] matchResults = new MatchResult[maxSuggestions];

        // keep track of the worst distance value in result list
        int worstDistanceValueInResults =
            maxAllowedDistance < tree.WordBufferLength - 1
                ? maxAllowedDistance + 1
                : tree.WordBufferLength;

        // traverse tree
        traversal.WalkTree((word) =>
        {
            distanceToInputWord = distanceAlgorithm.GetDistance(input, word);

            if (distanceToInputWord < worstDistanceValueInResults)
            {
                matchResults[indexOfMatchToBeReplacedNext] =
                    new MatchResult(word, distanceToInputWord);

                totalMatchesCount++;

                if (totalMatchesCount < maxSuggestions)
                {
                    indexOfMatchToBeReplacedNext++;
                }
                else
                {
                    worstDistanceValueInResults = 0;

                    for (int i = 0; i < maxSuggestions; ++i)
                    {
                        if (matchResults[i].GetMatchDistance() > worstDistanceValueInResults)
                        {
                            indexOfMatchToBeReplacedNext = i;
                            worstDistanceValueInResults = matchResults[i].GetMatchDistance();
                        }
                    }
                }
            }
        });

        matchesCount = Math.Min(totalMatchesCount, maxSuggestions);

        SortMatches(matchResults, matchesCount);

        Word[] matchedWords =
            matchResults
                .Where(result => result != null)
                .Select(result => result.GetMatchedWord())
                .ToArray();

        return new SuggestionResult(matchedWords, matchesCount, totalMatchesCount);
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
        private readonly int distance;

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
            return matchWord.ToString();
        }
    }
}