using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public unsafe abstract class WalkWordTreeService : IDisposable
    {
        protected WordTree tree;
        protected WordTreeNode[] stack;
        protected int stackIndex;
        protected char[] alphabetChars;
        protected int alphabetLength;

        public WalkWordTreeService(WordTree tree)
        {
            this.tree = tree;
            this.alphabetChars = tree.alphabet.GetChars();
            this.alphabetLength = tree.alphabet.GetLength();

            //initialize wordTree stack
            stack = new WordTreeNode[this.tree.metaData.wordBufferLength + 1];
            stackIndex = 0;
        
            tree.wordTreeWordBufferLengthChangedEventHandler += ((object sender, int newValue) =>
            {
                stack = new WordTreeNode[newValue + 1];
            });
        }
        public void Dispose()
        {

        }

        public abstract void WalkTree(Action<Word> onEachWord);

    }
}
