namespace SpellCheckingTool.Infrastructure.Servers.Exceptions;

public sealed class ServerShutdownException : Exception
{
    public ServerShutdownException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}