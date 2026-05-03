using SpellCheckingTool.Domain;

namespace SpellCheckingTool.Application.Executables;

public interface IExecutableParser
{
    public IWordStorage GetAllShellExecutables();
}
