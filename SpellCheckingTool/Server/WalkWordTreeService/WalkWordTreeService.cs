using System.Runtime.InteropServices;

namespace SpellCheckingTool
{
    public unsafe abstract class WalkWordTreeService : IDisposable
    {
        protected WordTree tree;
        protected WordTreeNode[] stack;
        protected int stackIndex;
        protected char* wordBuffer;
        protected int wordBufferIndex;
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

            //intialize native wordBuffer pointer
            wordBuffer = (char*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_malloc(sizeof(char) * tree.metaData.wordBufferLength) : API.linux_malloc(sizeof(char) * tree.metaData.wordBufferLength));
            wordBufferIndex = 0;

            //automatically resize stack and wordBuffer if the wordBufferLength property of the tree changes
            //this is okay, as this will not occur very frequently and the native realloc API would require stack to be a pointer, which is not possible in this setup (WordTreeNode is a class, hence "managed" (code whose execution lifecycle is managed by the .NET Runtime and its Garbage Collector (GC)), so it can't be a pointer)
            tree.wordTreeWordBufferLengthChangedEventHandler += ((object sender, int newValue) =>
            {
                stack = new WordTreeNode[newValue + 1];
                wordBuffer = (char*)(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? API.windows_realloc(wordBuffer, sizeof(char) * tree.metaData.wordBufferLength) : API.linux_realloc(wordBuffer, sizeof(char) * tree.metaData.wordBufferLength));
            });
        }
        public void Dispose()
        {
            if (wordBuffer != null)
            {
                //clean up native resources
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    API.windows_free(this.wordBuffer);
                else
                    API.linux_free(this.wordBuffer);

                //zero pointer
                wordBuffer = null;
            }
        }

    //    public abstract void WalkTree(Action<long, int> onEachWord);
        public abstract void WalkTree(Action<Word> onEachWord);

    }
}
