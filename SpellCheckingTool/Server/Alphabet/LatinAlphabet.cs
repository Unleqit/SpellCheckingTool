namespace SpellCheckingTool
{
    public class LatinAlphabet : BaseAlphabet
    {
        public LatinAlphabet() : base(new char[] { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' })
        {
            //nothing to do here, everything gets handled by the base class
        }

        //subtract the offsets of two chars in the ASCII table to get their location in the char array - only works for Latin alphabet, but significantly faster than mapping
        public override int GetCharPositionInArray(char c)
        {
            return (c - 'a');
        }

    }
}
