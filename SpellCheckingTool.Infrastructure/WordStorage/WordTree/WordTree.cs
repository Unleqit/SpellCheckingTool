using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Alphabet;

namespace SpellCheckingTool.Infrastructure;

public class WordTree : IWordStorage
{
    private WordTreeNode rootNode;
    private IAlphabet alphabet;
    private int wordCount;
    private int wordBufferLength;

    /// <summary>
    /// Creates an empty WordTree with the provided alphabet (or LatinAlphabet by default).
    /// </summary>
    public WordTree(IAlphabet? alphabet = null)
    {
        this.alphabet = alphabet ?? new LatinAlphabet();
        this.rootNode = new WordTreeNode(null, this.alphabet.GetLength(), false);
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
        int _wordCount = this.wordCount;
        int _wordBufferLength = this.wordBufferLength;

        foreach (Word word in words)
        {
            current = this.rootNode;

            //add each character of the current word to the tree structure
            for (posInWord = 0; posInWord < word.Length; ++posInWord)
            {
                posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
                if (current.Nodes[posInAlphabet] == null)
                    current.Nodes[posInAlphabet] = new WordTreeNode(current, alphabetLength, false);

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
        this.wordCount = _wordCount;
        this.wordBufferLength = _wordBufferLength;

        //return the amount of words that were successfully added to the tree structure
        return successCount;
    }

    public bool Contains(Word word)
    {
        WordTreeNode current = this.rootNode;
        int posInWord, posInAlphabet;

        for (posInWord = 0; posInWord < word.Length; ++posInWord)
        {
            posInAlphabet = alphabet.GetCharPositionInArray(word[posInWord]);
            if (posInAlphabet < 0 || current.Nodes[posInAlphabet] == null)
                return false;

            current = current.Nodes[posInAlphabet];
        }

        //required, as WordTree may contain words like 'lollipop', but not 'lol', therefore checks for 'lol' must fail
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
        int _wordCount = this.wordCount;

        foreach (Word word in words)
        {
            current = this.rootNode;

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
            if (current.Nodes[posInAlphabet] != null && current.Nodes[posInAlphabet].IsWord == true)
            {
                current.Nodes[posInAlphabet].IsWord = false;

            //check if node is empty - if so, rmeove it and check the parent node by traversing up the tree
            check:
                if (current != null
                    && current.Nodes[posInAlphabet] != null
                    && current.Nodes[posInAlphabet].Nodes.All(c => c == null)
                    && current.Nodes[posInAlphabet].IsWord == false)
                {
                    current.Nodes[posInAlphabet] = null;

                    //get position of next node in alphabet tree of this word
                    if (posInWord > 0)
                        posInAlphabet = alphabet.GetCharPositionInArray(word[--posInWord]);

                    //check parent node whether it is empty as well
                    current = current.parentNode;
                    if (current != null)
                        goto check;
                }

                successCount++;
            }

        //Note about this: this is a goto marker. Initially, it does nothing when the execution flow of the program begins entering the foreach loop.
        //When a word is invalid (e.g. there exists no node in the tree where one is expected, this entire word is skipped due to security reasons to prevent corrupting the tree
        skipThisWord:;
        }

        //We don't need to worry about reducing the 'WordBufferLength' property here, this property is only used for allocating the stack when walking the entire tree
        //and it scales proportional to O(log_<alphabet.Length>(n)).

        _wordCount -= successCount;
        this.wordCount = _wordCount;

        //return the number of words that were successfully removed from the tree
        return successCount;
    }

    public int GetWordCount()
    {
        return this.wordCount;
    }

    public IAlphabet GetAlphabet()
    {
        return this.alphabet;
    }

    public void Traverse(Action<Word> onEachWord)
    {
        char[] wordBuffer = new char[this.wordBufferLength + 1];
        int wordBufferIndex = 0;
        char[] alphabetChars = this.alphabet.GetChars();
        int alphabetLength = this.alphabet.GetLength();

        WordTreeNode[] stack = new WordTreeNode[this.wordBufferLength + 1];
        int stackIndex = 0;

        //we can't use recursion, as deep trees may cause a StackOverflowException, hence we walk the tree in an iterative manner.
        stack[stackIndex++] = this.rootNode;
        WordTreeNode current = this.rootNode;

        //the reserved property of each node in the tree gets "modified" on each traversal in order to keep track of all visited nodes.
        //resetting these to 0 for each node in the tree after each traversal would be too expensive, therefore this approach is employed:
        //The system has two states: The reserved property of the root node (and therefore all others in the tree) is 0 ("A run"), or it is 'alphabetLength' ("B run")
        //these values are used to dynamically flip the bounds of loops etc. depending on whether this traversal is an "A run", or a "B run"
        int reserved_maxValue = this.rootNode.reserved == 0 ? alphabetLength : 0;
        int reserved_incrementValue = this.rootNode.reserved == 0 ? 1 : -1;
        int reserved_invertedMaxValue = this.rootNode.reserved == 0 ? 0 : alphabetLength;

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
                for (int characterIndex = current.reserved;
                     characterIndex != reserved_maxValue;
                     characterIndex += reserved_incrementValue)
                {
                    int mappedIndex =
                        (characterIndex - reserved_invertedMaxValue) * reserved_incrementValue;

                    if (current.Nodes[mappedIndex] != null)
                    {
                        //the leftmost non-zero child pointer is traversed in order to find its children and pushed on the stack for backtracing
                        stack[stackIndex++] = current.Nodes[mappedIndex];
                        wordBuffer[wordBufferIndex++] = alphabetChars[mappedIndex];

                        //examine the next child on future visits, as this one is non-null
                        current.reserved = characterIndex + reserved_incrementValue;

                        //take a look at the leftmost child of this node and interrupt the traversal for this one
                        //the index of the first child node after this one that has yet to be checked is stored in the 'reserved' property (may be null)
                        current = current.Nodes[mappedIndex];
                        break;
                    }
                    else
                    {
                        //mark node as 'traversed', as it is null and contains no children
                        current.reserved = characterIndex + reserved_incrementValue;
                    }
                }
            }

            //check if nodes where all child nodes have been traversed represent a word themselves; if not, pop it from the stack and resume with the next node
            if (current.reserved == reserved_maxValue && current.IsWord != true)
            {
                stackIndex--;
                continue;
            }

            //call provided method for each word in the tree
            onEachWord(new Word(this.alphabet, wordBuffer, 0, wordBufferIndex));

            //the current node and its children have been fully examined and saved - pop it from the stack and resume with the next node
            stackIndex--;
        }
    }
}