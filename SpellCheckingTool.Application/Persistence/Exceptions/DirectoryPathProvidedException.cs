namespace SpellCheckingTool.Application.Persistence.Exceptions;

public sealed class DirectoryPathProvidedException : Exception
{
    public DirectoryPathProvidedException(string path)
        : base($"A directory path was provided where a file path was expected: '{path}'.")
    {
    }
}