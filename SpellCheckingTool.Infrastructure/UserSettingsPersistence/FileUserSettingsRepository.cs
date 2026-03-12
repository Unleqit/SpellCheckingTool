using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpellCheckingTool.Application.Settings;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpellCheckingTool.Infrastructure.UserSettingsPersistence
{
    public class FileUserSettingsRepository : IUserSettingsRepository
    {
        private readonly string _userSettingsFilePath;
        private readonly object _lock = new();
        private Dictionary<Guid, UserSettings> _userSettings = new();

        public FileUserSettingsRepository(string baseDirectory)
        {
            Directory.CreateDirectory(baseDirectory);
            _userSettingsFilePath = Path.Combine(baseDirectory, "usersettings.json");
            LoadUserSettings();
        }

        private void LoadUserSettings()
        {
            if (!File.Exists(_userSettingsFilePath))
            {
                _userSettings = new Dictionary<Guid, UserSettings>();
                return;
            }

            try
            {
                var json = File.ReadAllText(_userSettingsFilePath);

                var settings = new JsonSerializerSettings
                {
                    MissingMemberHandling = MissingMemberHandling.Ignore,
                    Converters = { new StringEnumConverter() }
                };

                _userSettings =
                    JsonConvert.DeserializeObject<Dictionary<Guid, UserSettings>>(json, settings)
                    ?? new Dictionary<Guid, UserSettings>();
            }
            catch
            {
                _userSettings = new Dictionary<Guid, UserSettings>();
            }
        }

        private void SaveUserSettings()
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new StringEnumConverter() }
            };

            var json = JsonConvert.SerializeObject(_userSettings, settings);
            File.WriteAllText(_userSettingsFilePath, json);
        }

        public UserSettings GetSettings(Guid userId)
        {
            lock (_lock)
            {
                if (_userSettings.TryGetValue(userId, out var settings))
                    return settings ?? UserSettings.Default;

                return UserSettings.Default;
            }
        }

        public void SetSettings(Guid userId, UserSettings settings)
        {
            lock (_lock)
            {
                _userSettings[userId] = settings;
                SaveUserSettings();
            }
        }
    }
}


