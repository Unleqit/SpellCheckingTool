using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;
using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.WordParser
{
    public class WordParser
    {
        public static Word[] ParseWords(IAlphabet alphabet, string[] rawWords)
        {
            List<Word> words = new List<Word>();
            for (int i = 0; i < rawWords.Length; ++i)
            {
                try
                {
                    Word word = new Word(alphabet, rawWords[i]);
                    words.Add(word);
                }
                catch (InvalidWordCharacterException)
                {
                    continue;
                }
                catch (InvalidWordRangeException)
                {
                    continue;
                }
            }
            return words.ToArray();
        }
    }
}
