namespace SpellCheckingTool
{
    public class LatinAlphabet : IAlphabet
    {
        public char[] GetChars() 
        {
            return new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };
        }

        public int GetLength() 
        {
            return 26; 
        }

        public int GetCharPositionInArray(char c)
        {
            return (c - 'a');
        }
    }
}
