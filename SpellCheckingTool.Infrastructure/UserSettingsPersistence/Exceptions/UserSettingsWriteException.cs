namespace SpellCheckingTool.Infrastructure.UserSettingsPersistence.Exceptions;

public sealed class UserSettingsWriteException : Exception
{
    public UserSettingsWriteException(string username, Exception innerException)
        : base($"Failed to save settings for user '{username}'.", innerException)
    {
    }
}