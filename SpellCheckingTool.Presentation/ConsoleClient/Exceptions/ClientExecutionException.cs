namespace SpellCheckingTool.Presentation.ConsoleClient.Exceptions;

public sealed class ClientExecutionException : Exception
{
    public ClientExecutionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}