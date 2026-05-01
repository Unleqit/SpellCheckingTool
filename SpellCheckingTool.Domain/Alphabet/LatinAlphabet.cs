namespace SpellCheckingTool.Domain.Alphabet;

public class LatinAlphabet : CustomAlphabet
{
    private static char[] latinChars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

    public LatinAlphabet() : base(latinChars)
    {

    }
}
