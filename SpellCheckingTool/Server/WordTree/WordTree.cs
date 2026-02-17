#pragma warning disable CS8625
#pragma warning disable CS8602


namespace SpellCheckingTool
{
    public class WordTree : IDisposable
    {
        public WordTreeNode rootNode { get; private set; }
        public IAlphabet alphabet { get; private set; }
        public WordTreeMetaInfo metaData { get; private set; }
        private IPersistenceService persistenceService;
        public event WordTreeWordBufferLengthChangedEventHandler? wordTreeWordBufferLengthChangedEventHandler;
        private ISuggestionService SuggestionService { get; set; }
        
#pragma warning disable CS8618 // all class members are set in the initialize method, hence the warnings about uninitialized variables can be ignored here
        public WordTree()
        {
            Initialize(null, null, null);
        }

        public WordTree(WordTreeParameters parameters)
        {
            Initialize(parameters.alphabet, parameters.persistenceService, parameters.distanceAlgorithm);
        }
#pragma warning restore CS8618

        private void Initialize(IAlphabet? alphabet, IPersistenceService? persistenceService, IDistanceAlgorithm? distanceAlgorithm)
        {
            this.alphabet = alphabet ?? new LatinAlphabet();
            this.rootNode = new WordTreeNode(null, this.alphabet.GetLength(), false);
            this.metaData = new WordTreeMetaInfo(0, 0, 0, 0);
            this.persistenceService = persistenceService ?? new FilePersistenceService(this);
            this.SuggestionService = new SuggestionService(this, distanceAlgorithm ?? new LevenshteinDistanceAlgorithm(this), new WalkWordTreeLeftToRightService(this));
        }

        public int Add(Word word)
        {
            return Add(new Word[] { word });
        }

        public int Add(Word[] words)
        {
            int successCount = 0;
            WordTreeNode current;
            int posInWord = 0;
            int posInAlphabet = 0;
            int totalWordLength = 0; //length of all words joined together with a seperator char ('\n')
            int alphabetLength = this.alphabet.GetLength();

            //cache global values to this method
            int _nodeCount = this.metaData.nodeCount;
            int _wordCount = this.metaData.wordCount;
            int _wordBufferLength = this.metaData.wordBufferLength;
            int _serializationLength = this.metaData.serializationLength;

            foreach (Word word in words)
            {
                current = this.rootNode;

                //add each character of the current word to the tree structure
                for (posInWord = 0; posInWord < word.Length; ++posInWord)
                {
                    posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                    if (current.Nodes[posInAlphabet] == null)
                    {
                        current.Nodes[posInAlphabet] = new WordTreeNode(current, alphabetLength, false);
                        _nodeCount++;
                    }

                    if (posInWord < word.Length - 1)
                        current = current.Nodes[posInAlphabet];
                }

                //check if the word already exists in the tree
                if (current.Nodes[posInAlphabet].IsWord == true)
                    continue;
                else
                {
                    //node is not marked as a word (e.g. "lollipop" is already contained in the word tree, and now "lol" is being added to it
                    successCount++;
                    current.Nodes[posInAlphabet].IsWord = true;
                    _wordBufferLength = _wordBufferLength < word.Length ? word.Length : _wordBufferLength;
                    totalWordLength += word.Length;
                }

                current = current.Nodes[posInAlphabet];
            }

            //update total word count in the tree
            _wordCount += successCount;

            //Update serialization length
            _serializationLength += totalWordLength + successCount;

            //write back cached values to tree structure
            this.metaData.wordCount = _wordCount;
            this.metaData.nodeCount = _nodeCount;
            this.metaData.serializationLength = _serializationLength;
            int oldLength = this.metaData.wordBufferLength;
            this.metaData.wordBufferLength = _wordBufferLength;

            //trigger event if the word buffer size has been expanded
            if (oldLength < _wordBufferLength)
                this.wordTreeWordBufferLengthChangedEventHandler?.Invoke(this, _wordBufferLength);

            //return the amount of words that were successfully added to the tree structure
            return successCount;
        }

        public bool Contains(string word)
        {
            // Create a Word object using the same alphabet as the tree
            try
            {
                Word wordObj = new Word(this.alphabet, word);
                return Contains(wordObj);
            }
            catch (Exception) 
            {
                //The provided word string could not be parsed into a Word object with the alphabet of the tree, therefore it cannot be contained in the tree
                return false;
            }
        }

        public bool Contains(Word word)
        {
            WordTreeNode current = this.rootNode;
            int posInWord, posInAlphabet;

            for (posInWord = 0; posInWord < word.Length; ++posInWord)
            {
                posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                if (current.Nodes[posInAlphabet] == null)
                    return false;

                current = current.Nodes[posInAlphabet];
            }

            //required, as wordlist may contain words like 'lollipop', but not 'lol', therefore checks for 'lol' must fail
            return current.IsWord;
        }

        public int Remove(Word word)
        {
            return Remove(new Word[] { word });
        }

        public int Remove(Word[] words)
        {
            int successCount = 0;
            WordTreeNode current = this.rootNode;
            int posInWord = 0;
            int posInAlphabet = 0;

            //cache global values to this method
            int _nodeCount = this.metaData.nodeCount;
            int _wordCount = this.metaData.wordCount;
            int _serializationLength = this.metaData.serializationLength;

            foreach (Word word in words)
            {
                //walk to the last character of the word in the tree
                for (posInWord = 0; posInWord < word.Length - 1; ++posInWord)
                {
                    posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                    if (current.Nodes[posInAlphabet] == null)
                        goto skipThisWord;

                    current = current.Nodes[posInAlphabet];
                }

                //handle last char
                posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                if (current.Nodes[posInAlphabet].IsWord == true)
                {
                    current.Nodes[posInAlphabet].IsWord = false;

                    //check if node is empty - if so, rmeove it and check the parent node by traversing up the tree
                    check:
                    if (current != null && current.Nodes[posInAlphabet].Nodes.All(c => c == null) && current.IsWord == false)
                    {
                        _nodeCount--;
                        current.Nodes[posInAlphabet] = null;

                        //get position of next node in alphabet tree of this word
                        if (posInWord > 0)
                            posInAlphabet = alphabet.GetCharPositionInArray(word[--posInWord]);

                        //check parent node whether it is empty as well
                        current = current.parentNode;
                        goto check;
                    }
                }

                successCount++;

                //update the serializelength property of the tree by subtracting the length of the removed word and its '\n' character
                _serializationLength -= word.Length + 1;

            //Note about this: this is a goto marker. Initially, it does nothing when the execution flow of the program begins entering the foreach loop.
            //When a word is invalid (e.g. there exists no node in the tree where one is expected, this entire word is skipped due to security reasons to prevent corrupting the tree
            skipThisWord: ;
            }

            //We don't need to worry about reducing the 'WordBufferLength' property here, this property is only used for allocating the stack when walking the entire tree
            //and it scales proportional to O(log_<alphabet.Length>(n)).

            _wordCount -= successCount;

            //write back cached values to tree structure
            this.metaData.wordCount = _wordCount;
            this.metaData.nodeCount = _nodeCount;
            this.metaData.serializationLength = _serializationLength;

            //return the number of words that were successfully removed from the tree
            return successCount;
        }

        public bool Serialize(FilePath filepath)
        {
            return this.persistenceService.Save(this, filepath);
        }

        public WordTree Deserialize(FilePath filepath)
        {
            return this.persistenceService.Load(filepath);
        }

        public SuggestionResult GetSuggestions(Word input, int maxAmountOfSuggestionsToBeReturned, int maxAllowedDistance)
        {
            return this.SuggestionService.GetSuggestionResult(input, maxAmountOfSuggestionsToBeReturned, maxAllowedDistance);
        }

        public void Dispose()
        {
            this.SuggestionService.Dispose();
        }
    }
}
