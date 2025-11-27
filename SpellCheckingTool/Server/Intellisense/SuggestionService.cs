namespace SpellCheckingTool
{
    unsafe class SuggestionService : ISuggestionService, IDisposable
    {
        WordTree tree;
        IDistanceAlgorithm distanceAlgorithm;
        WalkWordTreeService walkWordTreeService;
        char** matches;
        int* matchLengths;
        int* distanceOfMatchesToInput;

        public SuggestionService(WordTree tree, IDistanceAlgorithm distanceAlgorithm, WalkWordTreeService walkWordTreeService)
        {
            this.tree = tree;
            //get the distance algorithm to use for the creation of this suggestion
            this.distanceAlgorithm = distanceAlgorithm;
            this.walkWordTreeService = walkWordTreeService;

            this.matches = (char**)API.malloc(sizeof(char*) * 20);
            this.matchLengths = (int*)API.malloc(sizeof(int) * 20);
            this.distanceOfMatchesToInput = (int*)API.malloc(sizeof(int) * 20);

            for (int i = 0; i < 20; ++i)
                *(matches + i) = (char*)API.malloc(sizeof(char) * (tree.metaData.wordBufferLength + 1));

            //update buffer sizes on wordBufferSize change of tree
            tree.wordTreeWordBufferLengthChangedEventHandler += (object sender, int newSize) =>
            {
                for (int i = 0; i < 20; ++i)
                    *(matches + i) = (char*)API.realloc(*(matches + i), sizeof(char) * (tree.metaData.wordBufferLength + 1));
            };
        }

        /// <summary>
        /// Returns an object holding the best fit matches determined by the DistanceAlgorithm used in this instance.
        /// </summary>
        /// <param name="input">The input string to obtain matches for. Must be a single word.</param>
        /// <param name="maxAmountOfSuggestionsToBeReturned">The maximum amount of suggestions to return. Returns fewer suggestions if there are less than this upper limit.</param>
        /// <param name="maxAllowedDistance">The maximum allowed distance between the input word and a word from the tree for it to be considered a possible suggestion using the DistanceAlgorithm of this instance.</param>
        /// <returns></returns>
        public SuggestionResult GetSuggestionResult(string input, int maxAmountOfSuggestionsToBeReturned = 3, int maxAllowedDistance = 4)
        {
            int maxSuggestions = maxAmountOfSuggestionsToBeReturned < 0 ? 0 : maxAmountOfSuggestionsToBeReturned > 20 ? 20 : maxAmountOfSuggestionsToBeReturned;
            int maxWordLength = tree.metaData.wordBufferLength + 1;
            int distanceOfCurrentWordToInput = 0;
            int discoveredMatchesCount = 0;
            int indexOfMatchToBeReplacedNext = 0;

            //keep track of the worst distance value in result list
            int worstDistanceValueInResults = maxAllowedDistance < (tree.metaData.wordBufferLength - 1) ? (maxAllowedDistance + 1) : tree.metaData.wordBufferLength;

            //for each word in tree, update the matches buffer to maintain a collection of best matches for the current input as the tree gets traversed
            this.walkWordTreeService.WalkTree((long wordBuffer, int wordBufferLength) =>
            {
                fixed (char* pInput = input)
                    distanceOfCurrentWordToInput = distanceAlgorithm.GetDistance(pInput, (char*)wordBuffer, input.Length, wordBufferLength);

                //check if this word is to be considered as a possible suggestion. If so, override the least fit match in the matchResult buffer with the new match.
                if (distanceOfCurrentWordToInput < worstDistanceValueInResults)
                {
                    matchLengths[indexOfMatchToBeReplacedNext] = wordBufferLength;
                    for (int cpi = 0; cpi < wordBufferLength; ++cpi)
                        matches[indexOfMatchToBeReplacedNext][cpi] = *(((char*)wordBuffer) + cpi);

                    distanceOfMatchesToInput[indexOfMatchToBeReplacedNext] = distanceOfCurrentWordToInput;
                    discoveredMatchesCount++;

                    if (discoveredMatchesCount < maxSuggestions)
                        indexOfMatchToBeReplacedNext++;
                    else
                    {
                        worstDistanceValueInResults = 0;
                        for (int i = 0; i < maxSuggestions; ++i)
                        {
                            if (distanceOfMatchesToInput[i] > worstDistanceValueInResults)
                            {
                                indexOfMatchToBeReplacedNext = i;
                                worstDistanceValueInResults = distanceOfMatchesToInput[i];
                            }
                        }
                    }
                }
            });

            discoveredMatchesCount = discoveredMatchesCount < maxSuggestions ? discoveredMatchesCount : maxSuggestions;
            sortMatches(discoveredMatchesCount);

            SuggestionResult result = new SuggestionResult(matches, matchLengths, discoveredMatchesCount);
            return result;
        }

        void sortMatches(int matchCount)
        {
            char* matchToBeSwapped;
            int matchLengthToBeSwapped;
            int matchDistanceToBeSwapped;

            //sort the matches array by the distance of the matches in ascending order
            for (int i = 0; i < matchCount - 1; ++i)
                for (int j = 0; j < matchCount - i - 1; ++j)
                    if (distanceOfMatchesToInput[j] > distanceOfMatchesToInput[j + 1])
                    {
                        //swap match result in matches array
                        matchToBeSwapped = matches[j];
                        matches[j] = matches[j + 1];
                        matches[j + 1] = matchToBeSwapped;
                        matchLengthToBeSwapped = matchLengths[j];
                        matchLengths[j] = matchLengths[j + 1];
                        matchLengths[j + 1] = matchLengthToBeSwapped;

                        //swap match result in distance array
                        matchDistanceToBeSwapped = distanceOfMatchesToInput[j];
                        distanceOfMatchesToInput[j] = distanceOfMatchesToInput[j + 1];
                        distanceOfMatchesToInput[j + 1] = matchDistanceToBeSwapped;
                    }
        }

        public void Dispose()
        {
            walkWordTreeService.Dispose();
        }
    }
}
