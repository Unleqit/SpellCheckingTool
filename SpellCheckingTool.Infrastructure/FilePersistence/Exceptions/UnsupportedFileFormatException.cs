namespace SpellCheckingTool.Infrastructure.FilePersistence.Exceptions;

public sealed class UnsupportedFileFormatException : Exception
{
    public UnsupportedFileFormatException(string path)
        : base($"The file '{path}' is not supported. Only '.json' files are allowed.")
    {
    }
}