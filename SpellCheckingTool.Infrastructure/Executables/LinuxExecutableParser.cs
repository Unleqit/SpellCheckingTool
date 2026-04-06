using SpellCheckingTool.Domain.Alphabet;
using SpellCheckingTool.Domain;
using SpellCheckingTool.Application.WordParser;

namespace SpellCheckingTool.Infrastructure.Executables;
public class LinuxExecutableParser : ExecutableParser
{
    public override IWordStorage GetAllShellExecutables()
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", ".."));
        string workingDir = Path.Combine(projectRoot, "SpellCheckingTool.Infrastructure", "Executables");
        string filename = "listExecutables.sh";

        List<string> executablePaths = base.InvokeShellCommand("/bin/bash", filename, workingDir);

        IAlphabet alphabet = new ExecutableNameAlphabet();
        Word[] words = WordParser.ParseWords(alphabet, executablePaths.ToArray());
        WordTree tree = new WordTree(alphabet);

        tree.Add(words);
        return tree;
    }
}