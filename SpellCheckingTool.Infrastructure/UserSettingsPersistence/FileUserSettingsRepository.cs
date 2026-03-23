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
        private readonly object _lock = new();

        public FileUserSettingsRepository(string baseDirectory)
        {
            Directory.CreateDirectory(baseDirectory);
            _baseDirectory = baseDirectory;
        }

        public UserSettings GetSettings(string username)
        {
            lock (_lock)
            {
                var filePath = GetUserSettingsFilePath(username);

                if (!File.Exists(filePath))
                    return UserSettings.Default;

                try
                {
                    var json = File.ReadAllText(filePath);

                    var settings = new JsonSerializerSettings
                    {
                        MissingMemberHandling = MissingMemberHandling.Ignore,
                        Converters = { new StringEnumConverter() }
                    };

                    return JsonConvert.DeserializeObject<UserSettings>(json, settings)
                           ?? UserSettings.Default;
                }
                catch (IOException ex)
                {
                    Console.WriteLine(new UserSettingsReadException(username, ex).Message);
                    return UserSettings.Default;
                }
                catch (JsonException ex)
                {
                    Console.WriteLine(new UserSettingsReadException(username, ex).Message);
                    return UserSettings.Default;
                }
            }
        }

        public void SetSettings(string username, UserSettings settings)
        {
            lock (_lock)
            {
                var filePath = GetUserSettingsFilePath(username);

                var serializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    Converters = { new StringEnumConverter() }
                };

                var json = JsonConvert.SerializeObject(settings, serializerSettings);
                try
                {
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
        }

        private string GetUserSettingsFilePath(string username)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                username = username.Replace(c, '_');

            return Path.Combine(_baseDirectory, $"usersettings_{username}.json");
        }
    }
}


