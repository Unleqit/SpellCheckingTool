namespace SpellCheckingTool.Domain.Exceptions;

public abstract class SpellCheckingToolException : Exception
{
    protected SpellCheckingToolException(string message) : base(message) { }
    protected SpellCheckingToolException(string message, Exception innerException)
        : base(message, innerException) { }
}