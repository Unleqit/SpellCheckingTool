namespace SpellCheckingTool.Domain.Exceptions;

public sealed class InvalidSerializedAlphabetException : SpellCheckingToolException
{
    public InvalidSerializedAlphabetException()
        : base("The serialized alphabet data is invalid or corrupted.") { }

    public InvalidSerializedAlphabetException(string message) : base(message) { }
}