using SpellCheckingTool.Application.Settings;

namespace TestProject.Fakes;

public class FakeUserSettingsRepository : IUserSettingsRepository
{
    private readonly Dictionary<string, UserSettings> _settings =
        new(StringComparer.OrdinalIgnoreCase);

    public int SetSettingsCallCount { get; private set; }
    public string? LastSetUsername { get; private set; }
    public UserSettings? LastSetSettings { get; private set; }

    public UserSettings GetSettings(string username)
    {
        if (_settings.TryGetValue(username, out var settings))
            return settings;

        return UserSettings.Default;
    }

    public UserSettings GetDefaultSettings()
    {
        return UserSettings.Default;
    }

    public void SetSettings(string username, UserSettings settings)
    {
        SetSettingsCallCount++;
        LastSetUsername = username;
        LastSetSettings = settings;
        _settings[username] = settings;
    }

    public string GetUserSettingsFilePath(string? username)
    {
        return username ?? "default";
    }
}