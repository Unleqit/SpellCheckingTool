using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
