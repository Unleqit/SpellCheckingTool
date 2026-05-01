namespace SpellCheckingTool.Domain.Alphabet;

public class ExecutableNameAlphabet : CustomAlphabet
{
    private static char[] executableNameChars = "abcdefghijklmnopqrstuvwxyz0123456789_-".ToCharArray();

    public ExecutableNameAlphabet() : base(executableNameChars)
    {

    }
}
