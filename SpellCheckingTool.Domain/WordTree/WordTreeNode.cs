namespace SpellCheckingTool.Domain.WordTree;
    public class WordTreeNode
    {
        public WordTreeNode parentNode;
        public WordTreeNode[] Nodes { get; private set; }
        public bool IsWord { get; set; }
        public int reserved;

        public WordTreeNode(WordTreeNode parent, int length, bool isWord)
        {
            this.parentNode = parent;
            this.Nodes = new WordTreeNode[length];
            this.IsWord = isWord;
        }
    }
