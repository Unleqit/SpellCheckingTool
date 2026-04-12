using SpellCheckingTool.Domain.Alphabet;

namespace SpellCheckingTool.Domain;

public interface IWordStorage
{
    public int Add(Word word);
    public int Add(Word[] words);
    public bool Contains(Word word);
    public int Remove(Word word);
    public int Remove(Word[] words);
    public int GetWordCount();
    public IAlphabet GetAlphabet();
    public void Traverse(Action<Word> onEachWord);
}