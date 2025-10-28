#pragma warning disable CS8625
#pragma warning disable CS8602

namespace SpellCheckingTool
{
    public class WordTree
    {
        //nested class to prevent exposure of internal WordTreeNode structure
        class WordTreeNode
        {
            public WordTreeNode parentNode;
            public WordTreeNode[] Nodes { get; private set; }
            public bool IsWord { get; set; }

            public WordTreeNode(WordTreeNode parent, int length, bool isWord)
            {
                this.parentNode = parent;
                this.Nodes = new WordTreeNode[length];
                this.IsWord = isWord;
            }
        }

        private WordTreeNode rootNode;
        private IAlphabet alphabet;
        private int alphabetLength;
        public long Count { get; private set; }

        public WordTree(IAlphabet alphabet)
        {
            this.alphabet = alphabet;
            this.alphabetLength = this.alphabet.GetLength();
            this.rootNode = new WordTreeNode(null, this.alphabetLength, false);
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

            foreach (Word word in words)
            {
                current = this.rootNode;

                //handle all chars of a word except the last one
                for (posInWord = 0; posInWord < word.Length - 1; ++posInWord)
                {
                    posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                    if (current.Nodes[posInAlphabet] == null)
                        current.Nodes[posInAlphabet] = new WordTreeNode(current, this.alphabetLength, false);
                    current = current.Nodes[posInAlphabet];
                }

                //handle the last char and increase the successCount only, when the word is not already present in the tree
                posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                if (current.Nodes[posInAlphabet] == null)
                {
                    current.Nodes[posInAlphabet] = new WordTreeNode(current, this.alphabetLength, true);
                    successCount++;
                    continue;
                }

                if (current.Nodes[posInAlphabet].IsWord == true)
                    continue;
                else
                {
                    //end node of word already exists in tree, but is not marked as a word (e.g. "lollipop" is already contained in the word tree, and now "lol" is being added to it
                    successCount++;
                    current.Nodes[posInAlphabet].IsWord = true;
                }

                current = current.Nodes[posInAlphabet];

            }

            this.Count += successCount;
            return successCount;
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

            foreach (Word word in words)
            {
                //handle every char except the last one
                for (posInWord = 0; posInWord < word.Length - 1; ++posInWord)
                {
                    posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                    if (current.Nodes[posInAlphabet] == null)
                        goto fin;

                    current = current.Nodes[posInAlphabet];
                }

                //handle last char
                posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                if (current.Nodes[posInAlphabet].IsWord == true)
                {
                    current.Nodes[posInAlphabet].IsWord = false;

                    //check if node is empty - if so, rmeove it and check the parent node (TODO: clean this up)
                    check:
                    if (current != null && current.Nodes[posInAlphabet].Nodes.All(c => c == null) && current.IsWord == false)
                    {
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
            fin:;
            }

            this.Count -= successCount;
            return successCount;
        }
    }
}
