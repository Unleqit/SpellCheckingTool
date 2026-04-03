namespace SpellCheckingTool.Application.Settings
{
    public interface IUserSettingsRepository
    {
        UserSettings GetSettings(string username);
        UserSettings GetDefaultSettings();
        void SetSettings(string username, UserSettings settings);
        string GetUserSettingsFilePath(string? username);
    }
}
