using SpellCheckingTool.Application.SuggestionService;
using SpellCheckingTool.Application.WalkWordTreeService;
using SpellCheckingTool.Domain.DistanceAlgorithms;
using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Infrastructure.Suggestions;
    unsafe class SuggestionService : ISuggestionService
    {
        WordTree tree;
        IDistanceAlgorithm distanceAlgorithm;
        IWalkWordTreeService walkWordTreeService;

        public SuggestionService(WordTree tree, IDistanceAlgorithm distanceAlgorithm, IWalkWordTreeService walkWordTreeService)
        {
            this.tree = tree;
            this.distanceAlgorithm = distanceAlgorithm;
            this.walkWordTreeService = walkWordTreeService;
        }

        /// <summary>
        /// Returns an object holding the best fit matches determined by the DistanceAlgorithm used in this instance.
        /// </summary>
        /// <param name="input">The input string to obtain matches for. Must be a single word.</param>
        /// <param name="maxAmountOfSuggestionsToBeReturned">The maximum amount of suggestions to return. Returns fewer suggestions if there are less than this upper limit.</param>
        /// <param name="maxAllowedDistance">The maximum allowed distance between the input word and a word from the tree for it to be considered a possible suggestion using the DistanceAlgorithm of this instance.</param>
        /// <returns></returns>
        public SuggestionResult GetSuggestionResult(Word input, int maxAmountOfSuggestionsToBeReturned = 3, int maxAllowedDistance = 4)
        {
            int maxSuggestions = maxAmountOfSuggestionsToBeReturned < 0 ? 0 : maxAmountOfSuggestionsToBeReturned > 20 ? 20 : maxAmountOfSuggestionsToBeReturned;
            int distanceToInputWord = 0;
            int matchesCount = 0;
            int totalMatchesCount = 0;
            int indexOfMatchToBeReplacedNext = 0;

            MatchResult[] matchResults = new MatchResult[maxSuggestions];


            //keep track of the worst distance value in result list
            int worstDistanceValueInResults = maxAllowedDistance < (tree.metaData.wordBufferLength - 1) ? (maxAllowedDistance + 1) : tree.metaData.wordBufferLength;

            //for each word in tree, update the matches buffer to maintain a collection of best matches for the current input as the tree gets traversed
            this.walkWordTreeService.WalkTree((Word word) =>
            {
                distanceToInputWord = distanceAlgorithm.GetDistance(input, word);

                //check if this word is to be considered as a possible suggestion. If so, override the least fit match in the matchResult buffer with the new match.
                if (distanceToInputWord < worstDistanceValueInResults)
                {
                    matchResults[indexOfMatchToBeReplacedNext] = new MatchResult(word, distanceToInputWord);
                    totalMatchesCount++;

                    if (totalMatchesCount < maxSuggestions)
                        indexOfMatchToBeReplacedNext++;
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

            //ugly .NET solution of .reduce() in TypeScript
            Word[] matchStrings = matchResults.Where((result) => result != null).Select((result) => result.GetMatchedWord()).ToArray();

            SuggestionResult result = new SuggestionResult(matchStrings, matchesCount, totalMatchesCount);
            return result;
        }


        /// <summary>
        ///bubble sorts the matches array by their distances in ascending order
        /// </summary>
        /// <param name="results">the results array to be sorted in-place</param>
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
    }
