namespace SpellCheckingTool.Infrastructure.FilePersistence.Exceptions;

public sealed class TreeNotSpecifiedException : PersistenceException
{
    public TreeNotSpecifiedException()
        : base("The WordTree to persist was not provided.")
    {
    }
}