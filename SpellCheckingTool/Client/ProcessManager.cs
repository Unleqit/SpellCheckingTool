using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpellCheckingTool.Client
{
    public class ProcessManager
    {
        private readonly Process _process;

        public ProcessManager()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                    Console.WriteLine(e.Data);
            };

            _process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Data);
                    Console.ResetColor();
                }
            };

        }

        public void Start()
        {
            _process.Start();
            _process.StandardInput.AutoFlush = true;
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public void SendInput(string input)
        {
            _process.StandardInput.WriteLine(input);
        }
    }
}
