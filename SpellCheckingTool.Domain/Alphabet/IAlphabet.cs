namespace SpellCheckingTool.Domain.Alphabet;
    public interface IAlphabet
    {
        char[] GetChars();
        int GetLength();
        int GetCharPositionInArray(char c);
    }
