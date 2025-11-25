using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    unsafe class SuggestionService : ISuggestionService, IDisposable
    {
        WordTree tree;
        IDistanceAlgorithm distanceAlgorithm;
        WalkWordTreeService walkWordTreeService;

        public SuggestionService(WordTree tree, IDistanceAlgorithm distanceAlgorithm, WalkWordTreeService walkWordTreeService)
        {
            this.tree = tree;
            //get the distance algorithm to use for the creation of this suggestion
            this.distanceAlgorithm = distanceAlgorithm;
            this.walkWordTreeService = walkWordTreeService;
        }

        public SuggestionResult GetSuggestionResult(string input, int maxAmountOfSuggestionsToBeReturned = 3, int maxAllowedDistance = 4)
        {
            //create a new list to hold the results
            char** matches = (char**)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(char*) * maxAmountOfSuggestionsToBeReturned) : API.linux_malloc(sizeof(char*) * maxAmountOfSuggestionsToBeReturned));
            int* matchLengths = (int*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(int) * maxAmountOfSuggestionsToBeReturned) : API.linux_malloc(sizeof(int) * maxAmountOfSuggestionsToBeReturned));

            //initialize subpointers of matches buffer
            int maxWordLength = tree.metaData.wordBufferLength + 1;
            for (int i = 0; i < maxAmountOfSuggestionsToBeReturned; ++i)
                *(matches + i) = (char*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(char) * maxWordLength) : API.linux_malloc(sizeof(char) * maxWordLength));

            int distanceOfCurrentWordToInput = 0;
            int* distanceOfMatchesToInput = (int*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(int) * maxAmountOfSuggestionsToBeReturned) : API.linux_malloc(sizeof(int) * maxAmountOfSuggestionsToBeReturned));
            int discoveredMatchesCount = 0;
            int indexOfMatchToBeReplacedNext = 0;

            //keep track of the worst distance value in result list
            //add offset one in order to get exact matches (when "contain" is searched with maxDistance=0, we need to return "contain", but it would make so sense to replace with equally distanced matches in the if condition below
            int worstDistanceValueInResults = maxAllowedDistance < (tree.metaData.wordBufferLength - 1) ? (maxAllowedDistance + 1) : tree.metaData.wordBufferLength;

            //walk the tree using the provided WalkWordTreeService and execute the following lambda method for each word in the te
            this.walkWordTreeService.WalkTree((long wordBuffer, int wordBufferLength) =>
            {
                //get the distance of the current word in the tree from the provided input
                fixed (char* pInput = input)
                    distanceOfCurrentWordToInput = distanceAlgorithm.GetDistance(pInput, (char*)wordBuffer, input.Length, wordBufferLength);

                //check if this word is to be considered as a possible suggestion
                if (distanceOfCurrentWordToInput < worstDistanceValueInResults)
                {
                    //write current word to matchResult buffer (override the "least close" word there)
                    matchLengths[indexOfMatchToBeReplacedNext] = wordBufferLength;
                    for (int cpi = 0; cpi < wordBufferLength; ++cpi)
                        matches[indexOfMatchToBeReplacedNext][cpi] = *(((char*)wordBuffer) + cpi);

                    //update matchResultLD buffer to set the LD of this match
                    distanceOfMatchesToInput[indexOfMatchToBeReplacedNext] = distanceOfCurrentWordToInput;

                    //increment the amount of macthes discovered
                    discoveredMatchesCount++;

                    //what this does: first, populate the matches array with matching words fitting the defined minimum closeness to the input
                    //if there are more words like that in the tree, than the buffer can fit, we procedurally override the worst ones on the buffer with better matches
                    if (discoveredMatchesCount < maxAmountOfSuggestionsToBeReturned)
                        indexOfMatchToBeReplacedNext++;
                    else
                    {
                        worstDistanceValueInResults = 0;
                        for (int i = 0; i < maxAmountOfSuggestionsToBeReturned; ++i)
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

            //clamp the discovered matches count to the amount of matches we actually stored
            discoveredMatchesCount = discoveredMatchesCount < maxAmountOfSuggestionsToBeReturned ? discoveredMatchesCount : maxAmountOfSuggestionsToBeReturned;
            char* matchToBeSwapped;
            int matchLengthToBeSwapped;
            int matchDistanceToBeSwapped;

            //bubble sort the matches array, as it will only run once per request and the matchResultCount is probably < 5 anyway...
            for (int i = 0; i < discoveredMatchesCount - 1; ++i)
                for (int j = 0; j < discoveredMatchesCount - i - 1; ++j)
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

            //finally, we obtained an array of matches in the tree for the given input, so we wrap it in a class and return it
            SuggestionResult result = new SuggestionResult(matches, matchLengths, discoveredMatchesCount);
            return result;
        }

        public void Dispose()
        {
            walkWordTreeService.Dispose();
        }
    }
}
