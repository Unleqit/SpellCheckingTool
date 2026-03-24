using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Domain.Users;
using SpellCheckingTool.Infrastructure.UserSettingsPersistence.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;

namespace SpellCheckingTool.Infrastructure.UserSettingsPersistence
{
    public class FileUserSettingsRepository : IUserSettingsRepository
    {
        private readonly string _baseDirectory;
        private readonly UserSettings _defaultSettings;

        private const string DefaultSettingsFileName = "usersettings.json";

        private static readonly JsonSerializerSettings JsonOptions = new()
        {
            Formatting = Formatting.Indented,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            Converters = { new StringEnumConverter() }
        };

        public FileUserSettingsRepository(string baseDirectory)
        {
            Directory.CreateDirectory(baseDirectory);
            _baseDirectory = baseDirectory;
            _defaultSettings = InitializeDefaultSettings();
        }

        public UserSettings GetSettings(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return GetDefaultSettings();

            var filePath = GetUserSettingsFilePath(username);
            
            try
            {
                var settings = LoadFromFile(filePath);
                return settings ?? _defaultSettings;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(new UserSettingsReadException(username, ex).Message);
                return _defaultSettings;
            }
        }

        public UserSettings GetDefaultSettings()
        {
            var filePath = Path.Combine(_baseDirectory, DefaultSettingsFileName);
            
            try
            {
                return LoadFromFile(filePath) ?? UserSettings.Default;
            }
            catch (JsonException ex)
            {
                Console.WriteLine(new DefaultUserSettingsReadException(ex).Message);
                return UserSettings.Default;
            }
        }

        public void SetSettings(string username, UserSettings settings)
        {
            var filePath = GetUserSettingsFilePath(username);
            
            try
            {
                var json = JsonConvert.SerializeObject(settings, JsonOptions);
                File.WriteAllText(filePath, json);
            }
            catch (IOException ex)
            {
                throw new UserSettingsWriteException(username, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UserSettingsWriteException(username, ex);
            }
        }

        private UserSettings InitializeDefaultSettings()
        {
            var filePath = Path.Combine(_baseDirectory, DefaultSettingsFileName);

            var settings = LoadFromFile(filePath);

            if (settings != null)
                return settings;

            var json = JsonConvert.SerializeObject(UserSettings.Default, JsonOptions);
            File.WriteAllText(filePath, json);

            return UserSettings.Default;
        }

        private UserSettings? LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<UserSettings>(json, JsonOptions);
        }

        public string GetUserSettingsFilePath(string username)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                username = username.Replace(c, '_');

            return Path.Combine(_baseDirectory, $"usersettings_{username}.json");
        }
    }
}
