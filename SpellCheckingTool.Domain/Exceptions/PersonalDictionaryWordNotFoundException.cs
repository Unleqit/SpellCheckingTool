namespace SpellCheckingTool.Domain.Exceptions;

public sealed class PersonalDictionaryWordNotFoundException : SpellCheckingToolException
{
    public PersonalDictionaryWordNotFoundException(string word)
        : base($"The word '{word}' was not found in the personal dictionary.") { }
}