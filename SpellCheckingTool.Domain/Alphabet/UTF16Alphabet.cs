namespace SpellCheckingTool.Domain.Alphabet;

public class UTF16Alphabet : CustomAlphabet
{
    private static char[] utf16chars = new char[65536].Select((a, i) => (char)i).ToArray();

    public UTF16Alphabet() : base(utf16chars)
    {

    }
}

