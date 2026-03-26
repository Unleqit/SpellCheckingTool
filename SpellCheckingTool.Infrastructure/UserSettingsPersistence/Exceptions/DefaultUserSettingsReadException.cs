namespace SpellCheckingTool.Infrastructure.UserSettingsPersistence.Exceptions;

public sealed class DefaultUserSettingsReadException : Exception
{
    public DefaultUserSettingsReadException(Exception innerException)
        : base("Failed to read the default user settings file. Built-in defaults will be used.", innerException)
    {
    }
}
