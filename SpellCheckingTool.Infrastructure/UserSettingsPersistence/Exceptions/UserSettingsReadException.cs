namespace SpellCheckingTool.Infrastructure.UserSettingsPersistence.Exceptions;

public sealed class UserSettingsReadException : Exception
{
    public UserSettingsReadException(string username, Exception innerException)
        : base($"Failed to read settings for user '{username}'. Default settings will be used.", innerException)
    {
    }
}