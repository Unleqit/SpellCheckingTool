using SpellCheckingTool.Application.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpellCheckingTool.Infrastructure.UserSettingsPersistence
{
    public class FileOpener : IFileOpener
    {
        public void Open(string filePath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });
        }
    }
}
