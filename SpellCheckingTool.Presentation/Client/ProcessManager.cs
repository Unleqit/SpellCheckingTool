using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpellCheckingTool.Presentation.Client;
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
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/Q" : "",    // disable echo on Windows
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
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                input = @"bash -i -c 'printf ""%s"" ""${PS1@P}""' && " + input;
            _process.StandardInput.WriteLine(input);
        }
    }
