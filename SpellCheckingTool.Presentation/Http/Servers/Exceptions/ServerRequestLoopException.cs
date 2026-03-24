namespace SpellCheckingTool.Presentation.Servers.Exceptions;

public sealed class ServerRequestLoopException : Exception
{
    public ServerRequestLoopException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}