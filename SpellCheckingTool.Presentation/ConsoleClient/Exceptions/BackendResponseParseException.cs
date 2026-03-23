namespace SpellCheckingTool.Presentation.ConsoleClient.Exceptions;

public sealed class BackendResponseParseException : Exception
{
    public BackendResponseParseException(string operation, Exception innerException)
        : base($"Failed to parse backend response for '{operation}'.", innerException)
    {
    }
}