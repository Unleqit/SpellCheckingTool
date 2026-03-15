using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Presentation.ConsoleClient
{
    internal struct IndexedWord
    {
        public int startIndex;
        public int line;
        public int whiteSpaceAtEnd;
        public Word word;
        public int offset;

        public IndexedWord(int startIndex, int line, Word word)
        {
            this.startIndex = startIndex;
            this.line = line;
            this.word = word;
            this.whiteSpaceAtEnd = 0;
            this.offset = 0;
        }
    }
}
