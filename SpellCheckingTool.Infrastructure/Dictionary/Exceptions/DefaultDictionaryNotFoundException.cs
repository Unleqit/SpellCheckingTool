namespace SpellCheckingTool.Infrastructure.Dictionary.Exceptions;

public sealed class DefaultDictionaryNotFoundException : FileNotFoundException
{
    public DefaultDictionaryNotFoundException(string path)
        : base($"The default dictionary file was not found: '{path}'.")
    {
    }
}