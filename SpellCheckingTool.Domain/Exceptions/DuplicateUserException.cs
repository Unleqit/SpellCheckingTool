namespace SpellCheckingTool.Domain.Exceptions;

public sealed class DuplicateUserException : SpellCheckingToolException
{
    public DuplicateUserException(string username)
        : base($"A user with username '{username}' already exists.") { }

    public DuplicateUserException(Guid userId)
        : base($"A user with id '{userId}' already exists.") { }
}