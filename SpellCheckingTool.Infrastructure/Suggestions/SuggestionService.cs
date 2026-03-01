using SpellCheckingTool.Application.Suggestion;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Infrastructure.Suggestions;

public  class SuggestionService : ISuggestionService
{
    private readonly WordTree tree;
    private readonly IDistanceAlgorithm distanceAlgorithm;
    private readonly LeftToRightWordTreeTraversal traversal;

    public SuggestionService(WordTree tree, IDistanceAlgorithm distanceAlgorithm)
    {
        this.tree = tree;
        this.distanceAlgorithm = distanceAlgorithm;
        this.traversal = new LeftToRightWordTreeTraversal(tree);
    }

    /// <summary>
    /// Returns an object holding the best fit matches determined by the DistanceAlgorithm used in this instance.
    /// </summary>
    public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned = 3, int maxAllowedDistance = 4)
    {
        int maxSuggestions =
            maxAmountOfSuggestionsToBeReturned < 0 ? 0 :
            maxAmountOfSuggestionsToBeReturned > 20 ? 20 :
            maxAmountOfSuggestionsToBeReturned;

        int distanceToInputWord = 0;
        int matchesCount = 0;
        int totalMatchesCount = 0;
        int indexOfMatchToBeReplacedNext = 0;

        MatchResult[] matchResults = new MatchResult[maxSuggestions];

        // keep track of the worst distance value in result list
        int worstDistanceValueInResults =
            maxAllowedDistance < (tree.WordBufferLength - 1)
                ? (maxAllowedDistance + 1)
                : tree.WordBufferLength;

        // traverse tree
        this.traversal.WalkTree((Word word) =>
        {
            distanceToInputWord = distanceAlgorithm.GetDistance(input, word);

            if (distanceToInputWord < worstDistanceValueInResults)
            {
                matchResults[indexOfMatchToBeReplacedNext] = new MatchResult(word, distanceToInputWord);
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

        matchesCount = totalMatchesCount < maxSuggestions ? totalMatchesCount : maxSuggestions;

        sortMatches(matchResults, matchesCount);

        Word[] matchStrings =
            matchResults
                .Where(result => result != null)
                .Select(result => result.GetMatchedWord())
                .ToArray();

        return new SuggestionResult(matchStrings, matchesCount, totalMatchesCount);
    }

    /// <summary>
    /// bubble sorts the matches array by their distances in ascending order
    /// </summary>
    void sortMatches(MatchResult[] results, int matchesCount)
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
            return GetMatchedWord().ToString();
        }
    }
}