using SpellCheckingTool.Domain.Exceptions;

namespace SpellCheckingTool.Domain.Alphabet;

public class CustomAlphabet : IAlphabet
{
    private const int CHAR_COUNT = 65536;
    protected char[] chars;
    protected int length;
    private int[] mapping;

    public CustomAlphabet(char[] chars)
    {
        //check for duplicates
        if (chars.Distinct().Count() != chars.Length)
            throw new DuplicateAlphabetCharacterException();

        this.length = chars.Length;
        this.chars = chars;

        this.mapping = new int[CHAR_COUNT];
        for (int i = 0; i < CHAR_COUNT; ++i)
            mapping[i] = -1;

        //map chars to lookup array
        for (int i = 0; i < chars.Length; ++i)
            mapping[chars[i]] = i;
    }

    public char[] GetChars()
    {
        return chars;
    }

    public int GetLength()
    {
        return length;
    }

    public int GetCharPositionInArray(char c)
    {
        return this.mapping[c];
    }
}
