using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Infrastructure.Executables;
public interface IExecutableParser
    {
        public WordTree GetAllShellExecutables();
    }
