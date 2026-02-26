namespace SpellCheckingTool
{
    unsafe class SuggestionService : ISuggestionService
    {
        WordTree tree;
        IDistanceService distanceAlgorithm;
        IWalkWordTreeService walkWordTreeService;

        public SuggestionService(WordTree tree, IDistanceService distanceAlgorithm, IWalkWordTreeService walkWordTreeService)
        {
            this.tree = tree;
            this.distanceAlgorithm = distanceAlgorithm;
            this.walkWordTreeService = walkWordTreeService;
        }

        /// <summary>
        /// Returns an object holding the best fit matches determined by the DistanceAlgorithm used in this instance.
        /// </summary>
        /// <param name="input">The input string to obtain matches for. Must be a single word.</param>
        /// <param name="maxSuggestions">The maximum amount of suggestions to return. Returns fewer suggestions if there are less than this upper limit.</param>
        /// <param name="maxAllowedDistance">The maximum allowed distance between the input word and a word from the tree for it to be considered a possible suggestion using the DistanceAlgorithm of this instance.</param>
        /// <returns></returns>
        public SuggestionResult GetSuggestionResult(Word input, int maxSuggestions = 3, int maxAllowedDistance = 4)
        {
            int distanceToInputWord = 0;
            int matchDistance = 0;
            int totalMatchesCount = 0;
            int indexOfMatchToBeReplacedNext = 0;
            MatchResult[] matchResults = new MatchResult[maxSuggestions];

            this.walkWordTreeService.WalkTree((Word word) =>
            {
                distanceToInputWord = distanceAlgorithm.GetDistance(input, word);
                if (distanceToInputWord <= maxAllowedDistance)
                {
                    matchResults[indexOfMatchToBeReplacedNext] = new MatchResult(word, distanceToInputWord);
                    totalMatchesCount++;

                    if (totalMatchesCount < maxSuggestions)
                        indexOfMatchToBeReplacedNext++;
                    else
                    {
                        maxAllowedDistance = 0;
                        for (int i = 0; i < maxSuggestions; ++i)
                        {
                            matchDistance = matchResults[i].GetMatchDistance() - 1;
                            if (matchDistance > maxAllowedDistance)
                            {
                                indexOfMatchToBeReplacedNext = i;
                                maxAllowedDistance = matchDistance;
                            }
                        }
                    }
                }
            });

            int matchesCount = totalMatchesCount < maxSuggestions ? totalMatchesCount : maxSuggestions;
            sortMatches(matchResults, matchesCount);
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
}
