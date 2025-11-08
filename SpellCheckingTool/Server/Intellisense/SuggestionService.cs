using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    unsafe class SuggestionService : ISuggestionService
    {
        WordTree tree;
        WordTreeNode[] stack;
        int stackIndex;
        char* wordBuffer;
        int wordBufferIndex;
        IDistanceAlgorithm distanceAlgorithm;
        char[] alphabetChars;
        int alphabetLength;
        int worstDistanceValueInResults;

        public SuggestionService(WordTree tree)
        {
            this.tree = tree;
            alphabetChars = tree.alphabet.GetChars();
            alphabetLength = tree.alphabet.GetLength();

            //the two hardest things in computer science are cache invalidation, naming things, and off-by-one errors
            //we need the one extra space, as the root node of the tree gets passed as well
            stack = new WordTreeNode[tree.metaData.wordBufferLength + 1];
            stackIndex = 0;

            //automatically resize stack if the wordBufferLength property of the tree changes
            //this is okay, as this will not occur very frequently and the native realloc API would require stack to be a pointer, which is not possible in this setup (WordTreeNode is a class, hence "managed" (code whose execution lifecycle is managed by the .NET Runtime and its Garbage Collector (GC)), so it can't be a pointer)
            tree.wordTreeWordBufferLengthChangedEventHandler += ((object sender, int newValue) =>
            {
                stack = new WordTreeNode[newValue + 1];
            });

            //this is a native string used to extract each word from the tree and write it to the serialization file
            wordBuffer = (char*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(char) * tree.metaData.wordBufferLength) : API.linux_malloc(sizeof(char) * tree.metaData.wordBufferLength));
            wordBufferIndex = 0;

            //get the distance algorithm to use for the creation of this suggestion
            distanceAlgorithm = tree.DistanceAlgorithm;
        }

        public SuggestionResult GetSuggestionResult(string input, int maxAmountOfSuggestionsToBeReturned = 3, int maxAllowedDistance = 4)
        {
            //create a new list to hold the results
            string[] matches = new string[maxAmountOfSuggestionsToBeReturned];
            int distanceOfCurrentWordToInput = 0;
            int[] distanceOfMatchesToInput = new int[maxAmountOfSuggestionsToBeReturned];
            int discoveredMatchesCount = 0;
            int indexOfMatchToBeReplacedNext = 0;

            //keep track of the worst distance value in result list
            worstDistanceValueInResults = maxAllowedDistance < tree.metaData.wordBufferLength ? maxAllowedDistance : tree.metaData.wordBufferLength;

            //we can't use recursion, as deep trees may cause a StackOverflowException, hence we walk the tree in an iterative manner.
            //in order for this to work efficiently, without having to manage a deep stack, we leverage the 'WordBufferLength' property of the tree
            stack[stackIndex++] = tree.rootNode;
            WordTreeNode current = tree.rootNode;

            //the reserved property of each node in the tree gets "modified" on each traversal in order to keep track of all visited nodes.
            //resetting these to 0 for each node in the tree after each traversal would be too expensive, therefore this approach is employed:
            //The system has two states: The reserved property of the root node (and therefore all others in the tree) is 0 ("A run"), or it is 'alphabetLength' ("B run")
            //thse values are used to dynamically flip the bounds of loops etc. depending on whether this traversal is an "A run", or a "B run"
            int reserved_maxValue = tree.rootNode.reserved == 0 ? alphabetLength : 0;
            int reserved_incrementValue = tree.rootNode.reserved == 0 ? 1 : -1;
            int reserved_invertedMaxValue = tree.rootNode.reserved == 0 ? 0 : alphabetLength;

            //traverse the tree as long as the stack contains elements
            while (stackIndex > 0)
            {
                //all childs of this node were visited already, hence it can be popped
                if (current.reserved == reserved_maxValue)
                {
                    current = stack[stackIndex - 1];
                    --wordBufferIndex;
                }

                //traverse to bottom of tree to get first whole word
                //used is the bitwise property indicating which childs are in use, hence the value of the used property will be zero for leaf nodes without children 
                while (current.reserved != reserved_maxValue)
                {
                    //find smallest child of node which is yet to be serialized and save its index in the reserved property of the node
                    for (int characterIndex = current.reserved; characterIndex != reserved_maxValue; characterIndex += reserved_incrementValue)
                        if (current.Nodes[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue] != null)
                        {
                            //the leftmost non-zero child pointer is traversed in order to find its children and pushed on the stack for backtracing
                            stack[stackIndex++] = current.Nodes[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue];
                            wordBuffer[wordBufferIndex++] = alphabetChars[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue];

                            //examine the next child on future visits, as this one is non-null
                            current.reserved = characterIndex + reserved_incrementValue;

                            //take a look at the leftmost child of this node and interrupt the traversal for this one
                            //the index of the first child node after this one that has yet to be checked is stored in the 'reserved' property (may be null))
                            current = current.Nodes[(characterIndex - reserved_invertedMaxValue) * reserved_incrementValue];
                            break;
                        }
                        else
                            //mark note as 'traversed', as it is null and contains no children
                            current.reserved = characterIndex + reserved_incrementValue;
                }

                //check if nodes where all child nodes have been traversed represent a word themselves; if not, pop it from the stack and resume with the next node
                if (current.reserved == reserved_maxValue && current.IsWord != true)
                {
                    stackIndex--;
                    continue;
                }

                //get the distance of the current word in the tree from the provided input
                fixed (char* pInput = input)
                    distanceOfCurrentWordToInput = distanceAlgorithm.GetDistance(pInput, wordBuffer, input.Length, wordBufferIndex);

                //check if this word is to be considered as a possible suggestion
                if (distanceOfCurrentWordToInput < worstDistanceValueInResults)
                {
                    //write current word to matchResult buffer (override the "least close" word there
                    for (int cpi = 0; cpi < wordBufferIndex; ++cpi)
                        matches[indexOfMatchToBeReplacedNext] += *(wordBuffer + cpi);
                    matches[indexOfMatchToBeReplacedNext] += '\n';

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

                //the current node and its children have been fullt examined and saved - pop it from the stack and resume with the next node
                stackIndex--;
            }

            //clamp the discovered matches count to the amount of matches we actually stored
            discoveredMatchesCount = discoveredMatchesCount < maxAmountOfSuggestionsToBeReturned ? discoveredMatchesCount : maxAmountOfSuggestionsToBeReturned;
            string matchToBeSwapped;
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

                        //swap match result in distance array
                        matchDistanceToBeSwapped = distanceOfMatchesToInput[j];
                        distanceOfMatchesToInput[j] = distanceOfMatchesToInput[j + 1];
                        distanceOfMatchesToInput[j + 1] = matchDistanceToBeSwapped;
                    }

            //finally, we obtained an array of matches in the tree for the given input, so we wrap it in a class and return it
            SuggestionResult result = new SuggestionResult(matches, discoveredMatchesCount);
            return result;
        }
    }
}
