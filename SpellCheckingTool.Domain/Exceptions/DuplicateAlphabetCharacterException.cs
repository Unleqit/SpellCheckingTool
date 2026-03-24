namespace SpellCheckingTool.Domain.Exceptions;

public sealed class DuplicateAlphabetCharacterException : SpellCheckingToolException
{
    public DuplicateAlphabetCharacterException()
        : base("The alphabet contains duplicate characters.") { }

    public DuplicateAlphabetCharacterException(char duplicateChar)
        : base($"The alphabet contains the duplicate character '{duplicateChar}'.") { }
}