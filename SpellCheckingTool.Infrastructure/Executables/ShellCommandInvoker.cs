using SpellCheckingTool.Domain;
using System.Diagnostics;
using SpellCheckingTool.Application.Executables;

namespace SpellCheckingTool.Infrastructure.Executables;

public class ShellCommandInvoker
{
    public List<string> Execute(string shellName, string arguments, string workingDir = "")
    {
        List<string> executables = new List<string>();

        var psi = new ProcessStartInfo()
        {
            FileName = shellName,
            Arguments = arguments,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = workingDir
        };

        Process p = new Process();
        p.StartInfo = psi;


        p.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
        {
            if (e.Data != null)
                executables.Add(e.Data);
        };

        p.Start();
        p.BeginOutputReadLine();
        p.WaitForExit();

        return executables;
    }
}
