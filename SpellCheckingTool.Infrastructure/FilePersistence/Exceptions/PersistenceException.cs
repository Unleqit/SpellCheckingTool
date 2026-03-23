namespace SpellCheckingTool.Infrastructure.FilePersistence.Exceptions;

public abstract class PersistenceException : Exception
{
    protected PersistenceException(string message) : base(message) { }

    protected PersistenceException(string message, Exception innerException)
        : base(message, innerException) { }
}