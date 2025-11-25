using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public unsafe class LevenshteinDistanceAlgorithm : IDistanceAlgorithm
    {
        int maxWordLengthInTree;
        int* prev;
        int* current;
        int* _prev;
        int* _current;
        int* tmpBuffer;

        /// <summary>
        /// Provides an implementation of the levenshtein distance matching algorithm (see https://en.wikipedia.org/wiki/Levenshtein_distance)
        /// </summary>
#pragma warning disable CS8618 //_prev, _current and tmpBuffer are uninitialized after the constructor has been called, but that's okay, as the GetDistance algorithm sets these before accessing them
        public LevenshteinDistanceAlgorithm(WordTree tree)
#pragma warning restore CS8618
        {
            this.maxWordLengthInTree = tree.metaData.wordBufferLength + 1;
            int length = this.maxWordLengthInTree * sizeof(int);

            prev = (int*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(length) : API.linux_malloc(length));
            current = (int*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(length) : API.linux_malloc(length));

            //subscribe to the WordTreeWordBufferLengthChangedEventHandler in order to resize the native prev, next buffers when the word buffer length of the tree changes
            tree.wordTreeWordBufferLengthChangedEventHandler += ((object sender, int newWordLengthBufferSize) =>
            {
                this.maxWordLengthInTree = newWordLengthBufferSize + 1;
                int length = this.maxWordLengthInTree * sizeof(int);

                prev = (int*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_realloc(prev, length) : API.linux_realloc(prev, length));
                current = (int*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_realloc(current, length) : API.linux_realloc(current, length));
            });
        }

        /// <summary>
        /// Returns the Levenshtein distance between the two provided words.
        /// </summary>
        public int GetDistance(string wordA, string wordB)
        {
            int distance = 0;

            fixed (char* pWordA = wordA)
            fixed (char* pWordB = wordB)
                distance = GetDistance(pWordA, pWordB, wordA.Length, wordB.Length);

            return distance;
        }

        /// <summary>
        /// Returns the Levenshtein distance between the two provided words.
        /// </summary>
        public int GetDistance(char* wordA, char* wordB, int wordALength, int wordBLength)
        {
            int minimalCost;
            char* tmp;
            int tmpLen;

            //make sure s1 is the longer string
            if (wordBLength > wordALength)
            {
                tmp = wordA;
                tmpLen = wordALength;
                wordA = wordB;
                wordB = tmp;
                wordALength = wordBLength;
                wordBLength = tmpLen;
            }

            //populate the prev array with the cost of deletions
            for (int i = 0; i <= wordBLength; i++)
                prev[i] = i;

            //cahce references for prev and current
            _prev = prev;
            _current = current;

            //fill current array acording to levenshtein rules
            int deletionCost;
            int insertionCost;
            int substitutionCost;

            for (int i = 1; i <= wordALength; i++)
            {
                _current[0] = i;

                for (int j = 1; j <= wordBLength; j++)
                {
                    deletionCost = _prev[j] + 1;
                    insertionCost = _current[j - 1] + 1;
                    substitutionCost = _prev[j - 1] + ((wordA[i - 1] == wordB[j - 1]) ? 0 : 1);

                    minimalCost = deletionCost < insertionCost ? deletionCost : insertionCost;
                    _current[j] = minimalCost < substitutionCost ? minimalCost : substitutionCost;
                }

                //swap buffers
                tmpBuffer = _prev;
                _prev = _current;
                _current = tmpBuffer;
            }

            return _prev[wordBLength];
        }

        /// <summary>
        /// The size of the internal native buffers used for determining the Levenshtein distance between two words
        /// </summary>
        public int BufferSize
        {
            get { return maxWordLengthInTree; }
        }
    }
}
