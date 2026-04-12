using SpellCheckingTool.Domain;
using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;

namespace SpellCheckingTool.Infrastructure
{
    public class WordList : IWordStorage
    {
        private List<Word> list = new List<Word>();
        private IAlphabet alphabet;

        public WordList(IAlphabet alphabet)
        {
            this.alphabet = alphabet;
        }

        public int Add(Word word)
        {
            return _add([word]);
        }

        public int Add(Word[] words)
        {
            return _add(words);
        }

        private int _add(Word[] words)
        {
            char[] alphabetChars = alphabet.GetChars();
            int c = 0;
            foreach (var word in words)
                if (!this.list.Contains(word))
                {
                    if (word.ToString().Any((c) => !alphabetChars.Contains(c)))
                        throw new InvalidWordCharacterException(word.ToString(), alphabet);

                    c++;
                    this.list.Add(word);
                }

            return c;
        }

        public bool Contains(Word word)
        {
           return list.Contains(word);
        }

        public IAlphabet GetAlphabet()
        {
            return alphabet;
        }

        public int GetWordCount()
        {
            return list.Count;
        }

        public int Remove(Word word)
        {
            return _remove([word]);
        }

        public int Remove(Word[] words)
        {
            return _remove(words);
        }

        private int _remove(Word[] words)
        {
            foreach (var item in words)
                this.list.Remove(item);
                
            return words.Length;
        }

        public void Traverse(Action<Word> onEachWord)
        {
            foreach (var item in list)
                onEachWord(item);       
        }
    }
}
