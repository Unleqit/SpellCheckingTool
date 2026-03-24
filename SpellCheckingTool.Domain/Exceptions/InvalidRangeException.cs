namespace SpellCheckingTool.Domain.Exceptions;

public sealed class InvalidWordRangeException : SpellCheckingToolException
{
    public InvalidWordRangeException(string message) : base(message) { }
}