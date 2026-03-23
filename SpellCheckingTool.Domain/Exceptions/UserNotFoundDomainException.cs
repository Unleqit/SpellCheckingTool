namespace SpellCheckingTool.Domain.Exceptions;

public sealed class UserNotFoundDomainException : SpellCheckingToolException
{
    public UserNotFoundDomainException(Guid userId)
        : base($"User '{userId}' was not found.") { }

    public UserNotFoundDomainException(string username)
        : base($"User '{username}' was not found.") { }
}