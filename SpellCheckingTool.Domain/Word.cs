using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain.Exceptions;

namespace SpellCheckingTool.Domain;

public class Word
{
    private readonly char[] word;
    private readonly string originalFormatting;

    private Word()
    {
        this.word = [];
        this.originalFormatting = "";
    }

    public Word(IAlphabet alphabet, IEnumerable<char> word, int offset = 0, int length = -1)
    {
        char[] lowerCaseWord = GetLowerCaseArray(word, offset, length);
        this.CheckValidity(alphabet, lowerCaseWord);
        this.word = lowerCaseWord;
        this.originalFormatting = new string(word.Skip(offset).Take(length == -1 ? word.Count() : length).ToArray());
    }

    private void CheckValidity(IAlphabet alphabet, char[] word)
    {
        char[] alphabetChars = alphabet.GetChars();

        if (word.Any(c => !alphabetChars.Contains(c)))
            throw new InvalidWordCharacterException();
    }

    private char[] GetLowerCaseArray(IEnumerable<char> word, int offset = 0, int length = -1)
    {
        int passedWordLength = word.Count();

        if (length == -1)
            length = passedWordLength;

        if (offset < 0 || (length >= 0 && offset + length > passedWordLength))
            throw new InvalidWordRangeException(
            $"Invalid offset {offset} for input length {passedWordLength}.");

        if (length < 0 || (offset >= 0 && offset + length > passedWordLength))
            throw new InvalidWordRangeException(
            $"Invalid length {length} for offset {offset} and input length {passedWordLength}.");

        IEnumerable<char> substring = word.Skip(offset).Take(length);
        char[] lowerCaseWord = substring.Select((c) => char.ToLowerInvariant(c)).ToArray();
        return lowerCaseWord;
    }

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

    public string GetOriginalWordFormat()
    {
        return originalFormatting;
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
            return new Word(); 
        } 
    }

    public static bool operator ==(Word a, Word b)
    {
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.ToString() == b.ToString();
    }

    public static bool operator !=(Word a, Word b)
    {
        return !(a == b);
    }
}

