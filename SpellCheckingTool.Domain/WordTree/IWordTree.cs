using SpellCheckingTool.Domain.Alphabet;

namespace SpellCheckingTool.Domain.WordTree;

public interface IWordTree
{
    IAlphabet Alphabet { get; }
    int WordBufferLength { get; }
    int WordCount { get; }

    bool Contains(Word word);
}