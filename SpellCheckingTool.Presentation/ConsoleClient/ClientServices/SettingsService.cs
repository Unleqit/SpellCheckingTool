using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;
using System;
using System.IO;

namespace SpellCheckingTool.Presentation.ConsoleClient.ClientServices
{
    public class SettingsService
    {
        private readonly UserSpellcheckContext _context;
        private readonly IFileOpener _fileOpener;

        public SettingsService(UserSpellcheckContext context, IFileOpener fileOpener)
        {
            _context = context;
            _fileOpener = fileOpener;
        }   

        public string OpenOrCreateSettingsFile()
        {
            try
            {
                var username = _context.Username ?? string.Empty;
                var path = _context.SettingsRepository.GetUserSettingsFilePath(_context.Username);

                if (!File.Exists(path))
                {
                    var settingsToWrite = string.IsNullOrWhiteSpace(username)
                        ? _context.SettingsRepository.GetDefaultSettings()
                        : _context.Settings;

                    _context.SettingsRepository.SetSettings(username, settingsToWrite);
                }

                _fileOpener.Open(path);
                return $"Settings opened: {path}\nNote: Restart the application to apply changes.";
            }
            catch (Exception ex)
            {
                return $"Failed to open settings: {ex.Message}";
            }
        }
    }
}
