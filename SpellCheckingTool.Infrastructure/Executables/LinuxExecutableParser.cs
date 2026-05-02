using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain;
using SpellCheckingTool.Application.WordParser;
using SpellCheckingTool.Application.Executables;

namespace SpellCheckingTool.Infrastructure.Executables;

public class LinuxExecutableParser : IExecutableParser
{
    private readonly ShellCommandInvoker executor;
    
    public LinuxExecutableParser()
    {
        executor = new ShellCommandInvoker();
    }

    public IWordStorage GetAllShellExecutables()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        string workingDir = Path.Combine(projectRoot, "SpellCheckingTool.Infrastructure", "Executables");
        string filename = "listExecutables.sh";

        List<string> executablePaths = executor.Execute("/bin/bash", filename, workingDir);

        IAlphabet alphabet = new ExecutableNameAlphabet();
        Word[] words = WordParser.ParseWords(alphabet, executablePaths.ToArray());
        WordTree tree = new WordTree(alphabet);

        tree.Add(words);
        return tree;
    }
}