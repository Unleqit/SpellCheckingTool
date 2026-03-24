namespace SpellCheckingTool.Infrastructure.FilePersistence.Exceptions;

public sealed class WordTreeDeserializationException : Exception
{
    public WordTreeDeserializationException(string path)
        : base($"Failed to parse WordTree DTO from file '{path}'.")
    {
    }
}