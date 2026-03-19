using SpellCheckingTool.Domain.Alphabet;

namespace SpellCheckingTool.Domain.WordTree;
public class Word
{
    char[] word;

    public Word(IAlphabet alphabet, IEnumerable<char> word, int offset = 0, int length = -1)
    {
        char[] lowerCaseWord = GetLowerCaseArray(word, offset, length);
        this.CheckValidity(alphabet, lowerCaseWord);
        this.word = lowerCaseWord;

    }

    private void CheckValidity(IAlphabet alphabet, char[] word)
    {
        char[] alphabetChars = alphabet.GetChars();

        if (word.Any(c => !alphabetChars.Contains(c)))
            throw new Exception("Word contains invalid characters");
    }

    private char[] GetLowerCaseArray(IEnumerable<char> word, int offset = 0, int length = -1)
    {
        int passedWordLength = word.Count();

        if (length == -1)
            length = passedWordLength;

        if (offset < 0 || (length >= 0 && offset + length > passedWordLength))
            throw new Exception("Invalid offset specified");

        if (length < 0 || (offset >= 0 && offset + length > passedWordLength))
            throw new Exception("Invalid length specified");

        IEnumerable<char> substring = word.Skip(offset).Take(length);
        char[] lowerCaseWord = substring.Select((c) => char.ToLower(c)).ToArray();
        return lowerCaseWord;
    }

    //define array operator, length property and ToString()-Method on Word class for convenience
    public char this[int i]
    {
        get { return this.word[i]; }
    }

    public int Length
    {
        get { return word.Length; }
    }

    public override string ToString()
    {
        return new string(word);
    }

    //convenience method
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
#pragma warning disable CS0168
            catch (Exception e)
            {
                //just skip this word and do not add it to the list
                continue;
            }
#pragma warning restore CS0168
        }
        return words.ToArray();
    }

    public override bool Equals(object? obj)
    {
        return obj is Word other && word.SequenceEqual(other.word);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();

        foreach (char c in word)
        {
            hash.Add(c);
        }

        return hash.ToHashCode();
    }

    public static Word Empty 
    { 
        get
        {
            return new Word(new CustomAlphabet([]), "");
        } 
    }
}

