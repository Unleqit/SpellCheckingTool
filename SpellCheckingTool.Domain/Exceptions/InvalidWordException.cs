namespace SpellCheckingTool.Domain.Exceptions;

public sealed class InvalidWordException : SpellCheckingToolException
{
    public InvalidWordException(string message) : base(message) { }
    public InvalidWordException(string message, Exception innerException) : base(message, innerException) { }
}