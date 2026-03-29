using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Application.Executables;
public interface IExecutableParser
    {
        public WordTree GetAllShellExecutables();
    }
