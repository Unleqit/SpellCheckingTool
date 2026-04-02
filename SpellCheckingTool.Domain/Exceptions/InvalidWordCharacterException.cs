namespace SpellCheckingTool.Domain.Exceptions;

public sealed class InvalidWordCharacterException : SpellCheckingToolException
{
    public InvalidWordCharacterException()
        : base($"The given word contains one or more invalid characters.") { }

    public InvalidWordCharacterException(string word)
        : base($"The word '{word}' contains one or more characters that are not part of the alphabet.") { }
}