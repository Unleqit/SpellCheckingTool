using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpellCheckingTool.Presentation.ConsoleClient;
public class ShellProcessManager
{
    private readonly Process _process;
    public int CurrentShellOffset { get; private set; }
    private const string marker = "DONTPRINTTHIS";

    public ShellProcessManager()
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash",
                Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/Q /K @echo off" : "",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
        {
            if (e.Data != null && !e.Data.Contains(marker))
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
        KeepTrackOfCurrentWorkingDir();
    }

    private string currentWorkingDir = "";

    public void SendInput(string input)
    {
        _process.StandardInput.WriteLine(input);
        KeepTrackOfCurrentWorkingDir();
    }

    //wasted_hours = 5
    private void KeepTrackOfCurrentWorkingDir()
    {
        string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string markerWithTimeStamp = $"{marker}{timestamp}";
        string command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"echo %cd%{markerWithTimeStamp}" : $"echo \"$(pwd){markerWithTimeStamp}\"";

        string workingDir = "";
        DataReceivedEventHandler handler = (object sender, DataReceivedEventArgs e) =>
        {
            if (e.Data != null && e.Data.Contains(markerWithTimeStamp))
                workingDir = e.Data.Replace(markerWithTimeStamp, "");
        };

        _process.OutputDataReceived += handler;

        _process.StandardInput.WriteLine(command);

        while (workingDir == "")
            Thread.Sleep(1);

        currentWorkingDir = workingDir;
        _process.OutputDataReceived -= handler;
    }


    public string GetCurrentConsolePrompt()
    {
        string commandWindows = "/c echo %cd%^>";
        string commandLinux = "-i -c \"printf \\\"%s\\\" \\\"${PS1@P}\\\"\"";
        string command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? commandWindows : commandLinux;
        string shell = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "/bin/bash";

        ProcessStartInfo psi = new ProcessStartInfo(shell)
        {
            Arguments = command,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = currentWorkingDir
        };

#pragma warning disable CS8600
        Process process = Process.Start(psi);
#pragma warning restore CS8600


        if (process == null)
            return "";

        string output = process.StandardOutput.ReadToEnd();
        output = output.Replace(Environment.NewLine, "");

        CurrentShellOffset = output.Length;
        process.Dispose();

        return output;
    }
}
