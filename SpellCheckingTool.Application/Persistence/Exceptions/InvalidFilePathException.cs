namespace SpellCheckingTool.Application.Persistence.Exceptions;

public sealed class InvalidFilePathException : Exception
{
    public InvalidFilePathException(string path)
        : base($"The filepath is invalid because its directory does not exist: '{path}'.")
    {
    }
}