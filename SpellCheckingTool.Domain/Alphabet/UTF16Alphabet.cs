namespace SpellCheckingTool.Domain.Alphabet
{
    public class UTF16Alphabet : IAlphabet
    {
        CustomAlphabet internalAlphabet;

        public UTF16Alphabet()
        {
            char[] utf16chars = new char[65536];
            for (int i = 0; i < 65536; ++i)
                utf16chars[i] = (char)i;
            internalAlphabet = new CustomAlphabet(utf16chars);
        }

        public int GetCharPositionInArray(char c)
        {
            return internalAlphabet.GetCharPositionInArray(c);
        }

        public char[] GetChars()
        {
            return internalAlphabet.GetChars();
        }

        public int GetLength()
        {
            return internalAlphabet.GetLength();
        }
    }
}
