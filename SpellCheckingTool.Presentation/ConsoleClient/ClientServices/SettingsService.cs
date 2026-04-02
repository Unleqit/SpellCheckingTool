using SpellCheckingTool.Application.Settings;
using SpellCheckingTool.Application.Spellcheck;

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
            var path = GetSettingsFilePath();

            EnsureSettingsFileExists(path);

            _fileOpener.Open(path);

            return path;
        }
        private string GetSettingsFilePath()
        {
            return _context.SettingsRepository.GetUserSettingsFilePath(_context.Username);
        }

        private void EnsureSettingsFileExists(string path)
        {
            if (File.Exists(path))
                return;

            CreateSettingsFile();
        }

        private void CreateSettingsFile()
        {
            var username = _context.Username ?? string.Empty;
            var settingsToWrite = string.IsNullOrWhiteSpace(username)
                    ? _context.SettingsRepository.GetDefaultSettings()
                    : _context.Settings;

            _context.SettingsRepository.SetSettings(username, settingsToWrite);
        }
    }
}
