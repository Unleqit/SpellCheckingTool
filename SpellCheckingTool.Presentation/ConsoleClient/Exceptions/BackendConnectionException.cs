namespace SpellCheckingTool.Presentation.ConsoleClient.Exceptions;

public sealed class BackendConnectionException : Exception
{
    public BackendConnectionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}