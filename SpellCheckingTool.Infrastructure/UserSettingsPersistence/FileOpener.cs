using SpellCheckingTool.Application.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpellCheckingTool.Infrastructure.UserSettingsPersistence
{
    public class FileOpener : IFileOpener
    {
        public void Open(string filePath)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            else
            {
                Process.Start("nano", filePath);
            }
        }
    }
}
