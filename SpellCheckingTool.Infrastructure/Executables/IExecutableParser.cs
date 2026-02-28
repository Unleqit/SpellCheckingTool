using SpellCheckingTool.Domain.WordTree;

namespace SpellCheckingTool.Infrastructure.Executables;
    internal interface IExecutableParser
    {
        public WordTree GetAllShellExecutables();
    }
